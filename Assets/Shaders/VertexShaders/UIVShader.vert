#version 330 core

layout(location = 0) in vec3 vPos;
layout(location = 1) in vec2 vUV;

layout(location = 2) in vec2 vPosOffset;
layout(location = 3) in vec2 vSize;
layout(location = 4) in vec2 vUVOffset;
layout(location = 5) in vec2 vUVRange;
layout(location = 6) in vec4 vcolour;
layout(location = 7) in float layer;

uniform mat4 vpMatrix;

out vec2 fUV;
out vec4 fColour;
flat out int layerIndex;

void main(){
    vec2 pos = vPos.xy * vSize.xy;
    pos += vPosOffset;
    fUV = vUV * vUVRange + vUVOffset;
    fColour = vcolour;
    layerIndex = int(floor(layer));
    gl_Position = vpMatrix * vec4(pos.xy, vPos.z, 1);
}