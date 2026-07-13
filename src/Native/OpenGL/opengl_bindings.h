// Created By Levi Enama
// OpenGL Native Bindings Header for Open Engine

#ifndef OPEN_ENGINE_OPENGL_BINDINGS_H
#define OPEN_ENGINE_OPENGL_BINDINGS_H

#ifdef __cplusplus
extern "C" {
#endif

#include <stdint.h>

// Context management
typedef struct GLContext GLContext;

GLContext* GL_CreateContext(void* window, int width, int height, int major, int minor);
void GL_SetViewport(int x, int y, int width, int height);
void GL_ClearColor(float r, float g, float b, float a);
void GL_Clear(int mask);

// Shader management
typedef struct GLShader GLShader;

GLShader* GL_CreateShader(const char* vertexSource, const char* fragmentSource);
void GL_UseShader(GLShader* shader);
void GL_DeleteShader(GLShader* shader);

// Uniform management
void GL_SetUniformInt(GLShader* shader, const char* name, int value);
void GL_SetUniformFloat(GLShader* shader, const char* name, float value);
void GL_SetUniformVec3(GLShader* shader, const char* name, float x, float y, float z);
void GL_SetUniformVec4(GLShader* shader, const char* name, float x, float y, float z, float w);
void GL_SetUniformMat4(GLShader* shader, const char* name, float* matrix);

// Buffer management
typedef struct GLMesh GLMesh;

GLMesh* GL_CreateMesh(float* vertices, int vertexCount, unsigned int* indices, int indexCount);
void GL_DrawMesh(GLMesh* mesh);
void GL_DeleteMesh(GLMesh* mesh);

// Texture management
typedef struct GLTexture GLTexture;

GLTexture* GL_CreateTexture(unsigned char* data, int width, int height, int channels);
void GL_BindTexture(GLTexture* texture, int unit);
void GL_DeleteTexture(GLTexture* texture);

// Framebuffer management
typedef struct GLFramebuffer GLFramebuffer;

GLFramebuffer* GL_CreateFramebuffer(int width, int height);
void GL_BindFramebuffer(GLFramebuffer* fbo);
void GL_DeleteFramebuffer(GLFramebuffer* fbo);

// Cleanup
void GL_Shutdown(void);

// Clear masks
#define GL_COLOR_BUFFER_BIT 0x00004000
#define GL_DEPTH_BUFFER_BIT 0x00000100
#define GL_STENCIL_BUFFER_BIT 0x00000400

#ifdef __cplusplus
}
#endif

#endif // OPEN_ENGINE_OPENGL_BINDINGS_H
