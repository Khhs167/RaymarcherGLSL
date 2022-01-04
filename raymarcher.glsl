#version 430 core

// Number of threads in each workgroup
// the number of workroups dispatched is specified by us with a call to
// GL.DispatchCompute(x, x, x);
layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

// Binding is unit we will bind out texture to with
// GL.BindImageTexture
layout(binding = 0, rgba32f) restrict uniform image2D ImgResult;

void main()
{
    // Width and height of our image in Pixels
    ivec2 imgResultSize = imageSize(ImgResult);

    // gl_GlobalInvocationID is the current thread we are in
    // since we have a local size of 1 and we dispatch (Width, Height, 1)
    // this will be in the range of [0; Width][0, Height][0]
    ivec2 imgCoord = ivec2(gl_GlobalInvocationID.xy);

    vec2 color = imgCoord / vec2(imgResultSize); // range [0; 1]

    imageStore(ImgResult, imgCoord, vec4(color, 0.0, 1.0));
}