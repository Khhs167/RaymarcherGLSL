#version 430 core

// Number of threads in each workgroup
// the number of workroups dispatched is specified by us with a call to
// GL.DispatchCompute(x, x, x);
layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

// Binding is unit we will bind out texture to with
// GL.BindImageTexture
layout(binding = 0, rgba32f) restrict uniform image2D ImgResult;

const float nearPlane = 0.5;
const float farPlane = 1000;
const float collisionRange = 0.1;
uniform vec3 cameraPosition = vec3(0, 0, 0);
uniform mat4 cameraRotation;

struct Sphere{
  vec3 pos;
  vec3 color;
  float size;
};

const Sphere spheres[1] = { { vec3(0, 0, 10), vec3(1, 0, 0), 1 }};
const int sphereCount = 1;

void main()
{
  // Width and height of our image in Pixels
  ivec2 size = imageSize(ImgResult);
  
  // gl_GlobalInvocationID is the current thread we are in
  // since we have a local size of 1 and we dispatch (Width, Height, 1)
  // this will be in the range of [0; Width][0, Height][0]
  ivec2 imgCoord = ivec2(gl_GlobalInvocationID.xy);

  vec2 uv = imgCoord / vec2(size); // range [0; 1]
  vec3 color = vec3(-1, -1, -1);

  float ratio = float(size.y) / float(size.x);
  float d = 0;
  
  vec2 screenCoord = uv - vec2(0.5, 0.5);
  screenCoord.x /= ratio;

  vec3 cameraForward = vec3(0, 0, 1);
  vec3 cameraRight = vec3(1, 0, 0);
  vec3 cameraUp = vec3(0, 1, 0);
  vec3 worldCoord = (cameraForward * nearPlane) + (cameraUp * screenCoord.y) + (cameraRight * screenCoord.x);

  vec3 direction = -normalize((vec4(worldCoord, 0) * cameraRotation).xyz);

  while(d <= farPlane){

    if(color.x != -1){
      break;
    }

    vec3 pos = cameraPosition + (direction * d);
    float lowestDist = farPlane * 100;
    for(int i = 0; i < sphereCount; i++){
      Sphere sphere = spheres[i];
      lowestDist = min(lowestDist, distance(pos, sphere.pos) - sphere.size);
      if(lowestDist < collisionRange){
        color = sphere.color;
        break;
      }
    }
    d += lowestDist;
  }

  if(color.x == -1){
    color = vec3(0.2, 0.2, 1);
  }

  imageStore(ImgResult, imgCoord, vec4(color, 1.0));
}