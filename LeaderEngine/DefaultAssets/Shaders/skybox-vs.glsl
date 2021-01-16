﻿#version 330 core
layout (location = 0) in vec3 aPos;

out vec3 TexCoords;

uniform mat4 view;
uniform mat4 projection;

void main()
{
    TexCoords = aPos;
    gl_Position = vec4(aPos, 1.0) * mat4(mat3(view)) * projection;
} 