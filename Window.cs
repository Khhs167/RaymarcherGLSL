using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

namespace Raymarcher{
    public class Window : GameWindow
    {
        public const int RENDER_WIDTH = 800;
        public const int RENDER_HEIGHT = 600;
        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        ShaderProgram raymarcherProgram;
        Shader raymarcherCompute;

        ShaderProgram finalProgram;

        int computeTexture;
        int emptyVAO;

        protected override void OnLoad()
        {
            raymarcherCompute = new Shader("cs_raymarcher", File.ReadAllText("raymarcher.glsl"), ShaderType.ComputeShader);
            raymarcherProgram = new ShaderProgram("p_raymarcher", raymarcherCompute);

            finalProgram = new ShaderProgram("p_final", new Shader("vs_final", textureRenderVertex, ShaderType.VertexShader), new Shader("fs_final", textureRenderFrag, ShaderType.FragmentShader));

            GL.CreateTextures(TextureTarget.Texture2D, 1, out computeTexture);
            GL.TextureParameter(computeTexture, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TextureParameter(computeTexture, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
            GL.BindTexture(TextureTarget.Texture2D, computeTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 1, 1, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.BindImageTexture(0, computeTexture, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

            GL.CreateVertexArrays(1, out emptyVAO);
            GL.BindVertexArray(emptyVAO);
            

            base.OnLoad();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
          
            GL.UseProgram(raymarcherCompute.id);
            GL.BindImageTexture(0, computeTexture, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
            GL.DispatchCompute(1, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            GL.BindTextureUnit(0, computeTexture);
            GL.UseProgram(finalProgram.id);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            SwapBuffers();

            base.OnRenderFrame(args);
        }

string textureRenderVertex = @"
#version 430 core
const vec4 data[6] = vec4[]
(
    vec4( -1.0,  1.0,  0.0, 1.0 ),
    vec4( -1.0, -1.0,  0.0, 0.0 ),
    vec4(  1.0, -1.0,  1.0, 0.0 ),
    vec4( -1.0,  1.0,  0.0, 1.0 ),
    vec4(  1.0, -1.0,  1.0, 0.0 ),
    vec4(  1.0,  1.0,  1.0, 1.0 )
);

out InOutVars
{
    vec2 TexCoord;
} outData;

void main()
{
    vec4 vertex = data[gl_VertexID];

    gl_Position = vec4(vertex.xy, 0.0, 1.0);
    outData.TexCoord = vertex.zw;
}
";

string textureRenderFrag = @"
#version 430 core
layout(location = 0) out vec4 FragColor;
layout(binding = 0) uniform sampler2D Sampler;

in InOutVars
{
    vec2 TexCoord;
} inData;
void main()
{
    vec3 color = texture(Sampler, inData.TexCoord).rgb;

    FragColor = vec4(color, 1.0);
}
";

    }
}