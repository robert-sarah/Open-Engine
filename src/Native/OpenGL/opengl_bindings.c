// Created By Levi Enama
// OpenGL Native Bindings for Open Engine
// This file provides C bindings for OpenGL functionality

#include <GL/glew.h>
#include <stdio.h>
#include <stdlib.h>

// Context management
typedef struct {
    void* window;
    int width;
    int height;
    int majorVersion;
    int minorVersion;
} GLContext;

GLContext* g_currentContext = NULL;

// Initialize OpenGL context
GLContext* GL_CreateContext(void* window, int width, int height, int major, int minor) {
    GLContext* context = (GLContext*)malloc(sizeof(GLContext));
    context->window = window;
    context->width = width;
    context->height = height;
    context->majorVersion = major;
    context->minorVersion = minor;
    
    g_currentContext = context;
    
    // Initialize GLEW
    glewExperimental = GL_TRUE;
    if (glewInit() != GLEW_OK) {
        printf("Failed to initialize GLEW\n");
        free(context);
        return NULL;
    }
    
    printf("OpenGL %d.%d initialized\n", major, minor);
    return context;
}

// Set viewport
void GL_SetViewport(int x, int y, int width, int height) {
    glViewport(x, y, width, height);
}

// Clear buffers
void GL_ClearColor(float r, float g, float b, float a) {
    glClearColor(r, g, b, a);
}

void GL_Clear(int mask) {
    glClear(mask);
}

// Shader management
typedef struct {
    GLuint programId;
    GLuint vertexShader;
    GLuint fragmentShader;
} GLShader;

GLShader* GL_CreateShader(const char* vertexSource, const char* fragmentSource) {
    GLShader* shader = (GLShader*)malloc(sizeof(GLShader));
    
    // Compile vertex shader
    shader->vertexShader = glCreateShader(GL_VERTEX_SHADER);
    glShaderSource(shader->vertexShader, 1, &vertexSource, NULL);
    glCompileShader(shader->vertexShader);
    
    // Check vertex shader compilation
    GLint success;
    glGetShaderiv(shader->vertexShader, GL_COMPILE_STATUS, &success);
    if (!success) {
        char infoLog[512];
        glGetShaderInfoLog(shader->vertexShader, 512, NULL, infoLog);
        printf("Vertex shader compilation failed: %s\n", infoLog);
    }
    
    // Compile fragment shader
    shader->fragmentShader = glCreateShader(GL_FRAGMENT_SHADER);
    glShaderSource(shader->fragmentShader, 1, &fragmentSource, NULL);
    glCompileShader(shader->fragmentShader);
    
    // Check fragment shader compilation
    glGetShaderiv(shader->fragmentShader, GL_COMPILE_STATUS, &success);
    if (!success) {
        char infoLog[512];
        glGetShaderInfoLog(shader->fragmentShader, 512, NULL, infoLog);
        printf("Fragment shader compilation failed: %s\n", infoLog);
    }
    
    // Link shaders
    shader->programId = glCreateProgram();
    glAttachShader(shader->programId, shader->vertexShader);
    glAttachShader(shader->programId, shader->fragmentShader);
    glLinkProgram(shader->programId);
    
    // Check linking
    glGetProgramiv(shader->programId, GL_LINK_STATUS, &success);
    if (!success) {
        char infoLog[512];
        glGetProgramInfoLog(shader->programId, 512, NULL, infoLog);
        printf("Shader program linking failed: %s\n", infoLog);
    }
    
    return shader;
}

void GL_UseShader(GLShader* shader) {
    if (shader) {
        glUseProgram(shader->programId);
    }
}

void GL_DeleteShader(GLShader* shader) {
    if (shader) {
        glDeleteShader(shader->vertexShader);
        glDeleteShader(shader->fragmentShader);
        glDeleteProgram(shader->programId);
        free(shader);
    }
}

// Uniform management
void GL_SetUniformInt(GLShader* shader, const char* name, int value) {
    GLint location = glGetUniformLocation(shader->programId, name);
    glUniform1i(location, value);
}

void GL_SetUniformFloat(GLShader* shader, const char* name, float value) {
    GLint location = glGetUniformLocation(shader->programId, name);
    glUniform1f(location, value);
}

void GL_SetUniformVec3(GLShader* shader, const char* name, float x, float y, float z) {
    GLint location = glGetUniformLocation(shader->programId, name);
    glUniform3f(location, x, y, z);
}

void GL_SetUniformVec4(GLShader* shader, const char* name, float x, float y, float z, float w) {
    GLint location = glGetUniformLocation(shader->programId, name);
    glUniform4f(location, x, y, z, w);
}

void GL_SetUniformMat4(GLShader* shader, const char* name, float* matrix) {
    GLint location = glGetUniformLocation(shader->programId, name);
    glUniformMatrix4fv(location, 1, GL_FALSE, matrix);
}

// Buffer management
typedef struct {
    GLuint vao;
    GLuint vbo;
    GLuint ebo;
    int vertexCount;
    int indexCount;
} GLMesh;

GLMesh* GL_CreateMesh(float* vertices, int vertexCount, unsigned int* indices, int indexCount) {
    GLMesh* mesh = (GLMesh*)malloc(sizeof(GLMesh));
    mesh->vertexCount = vertexCount;
    mesh->indexCount = indexCount;
    
    glGenVertexArrays(1, &mesh->vao);
    glGenBuffers(1, &mesh->vbo);
    glGenBuffers(1, &mesh->ebo);
    
    glBindVertexArray(mesh->vao);
    
    // Vertex buffer
    glBindBuffer(GL_ARRAY_BUFFER, mesh->vbo);
    glBufferData(GL_ARRAY_BUFFER, vertexCount * sizeof(float), vertices, GL_STATIC_DRAW);
    
    // Index buffer
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, mesh->ebo);
    glBufferData(GL_ELEMENT_ARRAY_BUFFER, indexCount * sizeof(unsigned int), indices, GL_STATIC_DRAW);
    
    // Position attribute
    glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (void*)0);
    glEnableVertexAttribArray(0);
    
    // Normal attribute
    glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (void*)(3 * sizeof(float)));
    glEnableVertexAttribArray(1);
    
    // UV attribute
    glVertexAttribPointer(2, 2, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (void*)(6 * sizeof(float)));
    glEnableVertexAttribArray(2);
    
    glBindVertexArray(0);
    
    return mesh;
}

void GL_DrawMesh(GLMesh* mesh) {
    if (mesh) {
        glBindVertexArray(mesh->vao);
        glDrawElements(GL_TRIANGLES, mesh->indexCount, GL_UNSIGNED_INT, 0);
        glBindVertexArray(0);
    }
}

void GL_DeleteMesh(GLMesh* mesh) {
    if (mesh) {
        glDeleteVertexArrays(1, &mesh->vao);
        glDeleteBuffers(1, &mesh->vbo);
        glDeleteBuffers(1, &mesh->ebo);
        free(mesh);
    }
}

// Texture management
typedef struct {
    GLuint textureId;
    int width;
    int height;
    int channels;
} GLTexture;

GLTexture* GL_CreateTexture(unsigned char* data, int width, int height, int channels) {
    GLTexture* texture = (GLTexture*)malloc(sizeof(GLTexture));
    texture->width = width;
    texture->height = height;
    texture->channels = channels;
    
    glGenTextures(1, &texture->textureId);
    glBindTexture(GL_TEXTURE_2D, texture->textureId);
    
    GLenum format = GL_RGB;
    if (channels == 4) format = GL_RGBA;
    else if (channels == 1) format = GL_RED;
    
    glTexImage2D(GL_TEXTURE_2D, 0, format, width, height, 0, format, GL_UNSIGNED_BYTE, data);
    
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    
    glGenerateMipmap(GL_TEXTURE_2D);
    
    glBindTexture(GL_TEXTURE_2D, 0);
    
    return texture;
}

void GL_BindTexture(GLTexture* texture, int unit) {
    if (texture) {
        glActiveTexture(GL_TEXTURE0 + unit);
        glBindTexture(GL_TEXTURE_2D, texture->textureId);
    }
}

void GL_DeleteTexture(GLTexture* texture) {
    if (texture) {
        glDeleteTextures(1, &texture->textureId);
        free(texture);
    }
}

// Framebuffer management
typedef struct {
    GLuint fbo;
    GLuint texture;
    GLuint rbo;
    int width;
    int height;
} GLFramebuffer;

GLFramebuffer* GL_CreateFramebuffer(int width, int height) {
    GLFramebuffer* fbo = (GLFramebuffer*)malloc(sizeof(GLFramebuffer));
    fbo->width = width;
    fbo->height = height;
    
    glGenFramebuffers(1, &fbo->fbo);
    glBindFramebuffer(GL_FRAMEBUFFER, fbo->fbo);
    
    // Color attachment
    glGenTextures(1, &fbo->texture);
    glBindTexture(GL_TEXTURE_2D, fbo->texture);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, NULL);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, fbo->texture, 0);
    
    // Depth attachment
    glGenRenderbuffers(1, &fbo->rbo);
    glBindRenderbuffer(GL_RENDERBUFFER, fbo->rbo);
    glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH24_STENCIL8, width, height);
    glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_RENDERBUFFER, fbo->rbo);
    
    if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE) {
        printf("Framebuffer is not complete\n");
    }
    
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
    
    return fbo;
}

void GL_BindFramebuffer(GLFramebuffer* fbo) {
    if (fbo) {
        glBindFramebuffer(GL_FRAMEBUFFER, fbo->fbo);
    } else {
        glBindFramebuffer(GL_FRAMEBUFFER, 0);
    }
}

void GL_DeleteFramebuffer(GLFramebuffer* fbo) {
    if (fbo) {
        glDeleteFramebuffers(1, &fbo->fbo);
        glDeleteTextures(1, &fbo->texture);
        glDeleteRenderbuffers(1, &fbo->rbo);
        free(fbo);
    }
}

// Cleanup
void GL_Shutdown() {
    if (g_currentContext) {
        free(g_currentContext);
        g_currentContext = NULL;
    }
}
