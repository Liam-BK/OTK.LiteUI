# version 330 core

in vec2 fUV;
in vec4 fColour;
flat in int layerIndex;

out vec4 FragColor;

uniform sampler2DArray terrainTexture;

void main(){
    FragColor = layerIndex == -1 ? fColour : texture(terrainTexture, vec3(fUV, layerIndex));
}