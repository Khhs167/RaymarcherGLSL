#version 430 core
layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

layout(binding = 0, rgba32f) restrict uniform image2D ImgResult;

void main() {
  vec4 pixel = vec4(1.0, 1.0, 1.0, 1.0);

  ivec2 pixel_coords = ivec2(gl_GlobalInvocationID.xy);

  imageStore(ImgResult, pixel_coords, pixel);
}