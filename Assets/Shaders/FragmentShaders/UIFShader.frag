# version 330 core

in vec2 fUV;
in vec4 fColour;
flat in int layerIndex;

out vec4 FragColor;

uniform sampler2DArray uTexture;

void main(){
    FragColor = layerIndex == -1 ? fColour : fColour * texture(uTexture, vec3(fUV, layerIndex));
}