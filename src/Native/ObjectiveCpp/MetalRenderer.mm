// Created By Levi Enama
// Metal Renderer for iOS/macOS (Objective-C++)
// This file provides Metal rendering bindings for Open Engine

#import <Metal/Metal.h>
#import <QuartzCore/CAMetalLayer.h>
#import <Foundation/Foundation.h>
#include <stdio.h>
#include <stdlib.h>

@interface MetalRenderer : NSObject
@property (nonatomic, strong) id<MTLDevice> device;
@property (nonatomic, strong) id<MTLCommandQueue> commandQueue;
@property (nonatomic, strong) id<MTLRenderPipelineState> pipelineState;
@property (nonatomic, strong) CAMetalLayer *metalLayer;
@property (nonatomic, strong) id<MTLBuffer> vertexBuffer;
@property (nonatomic, strong) id<MTLBuffer> indexBuffer;
@property (nonatomic, assign) NSInteger vertexCount;
@property (nonatomic, assign) NSInteger indexCount;
@end

@implementation MetalRenderer

- (instancetype)init
{
    self = [super init];
    if (self) {
        [self initializeMetal];
    }
    return self;
}

- (void)initializeMetal
{
    // Get the default Metal device
    self.device = MTLCreateSystemDefaultDevice();
    if (!self.device) {
        printf("Metal is not supported on this device\n");
        return;
    }

    // Create command queue
    self.commandQueue = [self.device newCommandQueue];

    printf("Metal initialized successfully\n");
}

- (void)setupMetalLayer:(CALayer*)layer
{
    self.metalLayer = (CAMetalLayer*)layer;
    self.metalLayer.device = self.device;
    self.metalLayer.pixelFormat = MTLPixelFormatBGRA8Unorm;
    self.metalLayer.framebufferOnly = YES;
}

- (void)createPipelineWithVertexShader:(NSString*)vertexSource
                       fragmentShader:(NSString*)fragmentSource
{
    NSError *error = nil;

    id<MTLLibrary> defaultLibrary = [self.device newLibraryWithSource:vertexSource
                                                               options:nil
                                                                 error:&error];
    if (error) {
        printf("Failed to create vertex shader library: %s\n", [[error localizedDescription] UTF8String]);
        return;
    }

    id<MTLFunction> vertexFunction = [defaultLibrary newFunctionWithName:@"vertexShader"];

    defaultLibrary = [self.device newLibraryWithSource:fragmentSource
                                              options:nil
                                                error:&error];
    if (error) {
        printf("Failed to create fragment shader library: %s\n", [[error localizedDescription] UTF8String]);
        return;
    }

    id<MTLFunction> fragmentFunction = [defaultLibrary newFunctionWithName:@"fragmentShader"];

    // Create pipeline descriptor
    MTLRenderPipelineDescriptor *pipelineDescriptor = [[MTLRenderPipelineDescriptor alloc] init];
    pipelineDescriptor.label = @"Simple Pipeline";
    pipelineDescriptor.vertexFunction = vertexFunction;
    pipelineDescriptor.fragmentFunction = fragmentFunction;
    pipelineDescriptor.colorAttachments[0].pixelFormat = self.metalLayer.pixelFormat;

    // Create pipeline state
    self.pipelineState = [self.device newRenderPipelineStateWithDescriptor:pipelineDescriptor
                                                                     error:&error];
    if (error) {
        printf("Failed to create pipeline state: %s\n", [[error localizedDescription] UTF8String]);
    }
}

- (void)createVertexBuffer:(float*)vertices count:(NSInteger)count
{
    self.vertexCount = count;
    self.vertexBuffer = [self.device newBufferWithBytes:vertices
                                                 length:count * sizeof(float)
                                                options:MTLResourceStorageModeManaged];
}

- (void)createIndexBuffer:(uint32_t*)indices count:(NSInteger)count
{
    self.indexCount = count;
    self.indexBuffer = [self.device newBufferWithBytes:indices
                                                length:count * sizeof(uint32_t)
                                               options:MTLResourceStorageModeManaged];
}

- (void)render
{
    @autoreleasepool {
        id<MTLCommandBuffer> commandBuffer = [self.commandQueue commandBuffer];

        MTLRenderPassDescriptor *renderPassDescriptor = self.metalLayer.currentRenderPassDescriptor;
        if (renderPassDescriptor == nil) {
            return;
        }

        id<MTLRenderCommandEncoder> renderEncoder = [commandBuffer renderCommandEncoderWithDescriptor:renderPassDescriptor];
        [renderEncoder setRenderPipelineState:self.pipelineState];

        if (self.vertexBuffer) {
            [renderEncoder setVertexBuffer:self.vertexBuffer offset:0 atIndex:0];
        }

        if (self.indexBuffer && self.indexCount > 0) {
            [renderEncoder drawIndexedPrimitives:MTLPrimitiveTypeTriangle
                                     indexCount:self.indexCount
                                      indexType:MTLIndexTypeUInt32
                                    indexBuffer:self.indexBuffer
                              indexBufferOffset:0];
        } else if (self.vertexCount > 0) {
            [renderEncoder drawPrimitives:MTLPrimitiveTypeTriangle vertexStart:0 vertexCount:self.vertexCount];
        }

        [renderEncoder endEncoding];

        [commandBuffer presentDrawable:self.metalLayer.nextDrawable];
        [commandBuffer commit];
    }
}

- (void)cleanup
{
    self.device = nil;
    self.commandQueue = nil;
    self.pipelineState = nil;
    self.vertexBuffer = nil;
    self.indexBuffer = nil;
}

@end

// C interface for C# interop
extern "C" {
    typedef void* MetalRendererRef;

    MetalRendererRef Metal_CreateRenderer() {
        MetalRenderer* renderer = [[MetalRenderer alloc] init];
        return (__bridge_retained void*)renderer;
    }

    void Metal_DestroyRenderer(MetalRendererRef rendererRef) {
        MetalRenderer* renderer = (__bridge_transfer MetalRenderer*)rendererRef;
        [renderer cleanup];
    }

    void Metal_SetupLayer(MetalRendererRef rendererRef, void* layer) {
        MetalRenderer* renderer = (__bridge MetalRenderer*)rendererRef;
        [renderer setupMetalLayer:(__bridge CALayer*)layer];
    }

    void Metal_CreatePipeline(MetalRendererRef rendererRef, const char* vertexSource, const char* fragmentSource) {
        MetalRenderer* renderer = (__bridge MetalRenderer*)rendererRef;
        NSString* vertexStr = [NSString stringWithUTF8String:vertexSource];
        NSString* fragmentStr = [NSString stringWithUTF8String:fragmentSource];
        [renderer createPipelineWithVertexShader:vertexStr fragmentShader:fragmentStr];
    }

    void Metal_CreateVertexBuffer(MetalRendererRef rendererRef, float* vertices, int count) {
        MetalRenderer* renderer = (__bridge MetalRenderer*)rendererRef;
        [renderer createVertexBuffer:vertices count:count];
    }

    void Metal_CreateIndexBuffer(MetalRendererRef rendererRef, uint32_t* indices, int count) {
        MetalRenderer* renderer = (__bridge MetalRenderer*)rendererRef;
        [renderer createIndexBuffer:indices count:count];
    }

    void Metal_Render(MetalRendererRef rendererRef) {
        MetalRenderer* renderer = (__bridge MetalRenderer*)rendererRef;
        [renderer render];
    }
}
