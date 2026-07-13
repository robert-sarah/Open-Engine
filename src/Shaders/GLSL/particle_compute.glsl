// Created By Levi Enama
// Particle System Compute Shader
#version 450 core

layout(local_size_x = 128) in;

struct Particle {
    vec4 position;
    vec4 velocity;
    vec4 color;
    float life;
    float size;
};

layout(std430, binding = 0) buffer ParticleBuffer {
    Particle particles[];
};

uniform float deltaTime;
uniform vec3 gravity;
uniform vec3 emitterPosition;
uniform vec3 emitterDirection;
uniform float emissionRate;
uniform float particleLife;
uniform int particleCount;
uniform float time;

// Random number generator
float rand(vec2 co) {
    return fract(sin(dot(co.xy, vec2(12.9898, 78.233))) * 43758.5453);
}

void main()
{
    uint index = gl_GlobalInvocationID.x;

    if (index >= particleCount) return;

    Particle p = particles[index];

    // Update particle
    if (p.life > 0.0) {
        // Apply gravity
        p.velocity.xyz += gravity * deltaTime;

        // Update position
        p.position.xyz += p.velocity.xyz * deltaTime;

        // Decrease life
        p.life -= deltaTime;

        // Fade out
        p.color.a = p.life / particleLife;
    }
    else {
        // Respawn particle
        if (rand(vec2(index, time)) < emissionRate * deltaTime) {
            p.position.xyz = emitterPosition;
            p.velocity.xyz = emitterDirection + vec3(
                rand(vec2(index, time)) - 0.5,
                rand(vec2(index, time + 1.0)) - 0.5,
                rand(vec2(index, time + 2.0)) - 0.5
            ) * 2.0;
            p.life = particleLife;
            p.color = vec4(1.0, 1.0, 1.0, 1.0);
            p.size = 1.0;
        }
    }

    particles[index] = p;
}
