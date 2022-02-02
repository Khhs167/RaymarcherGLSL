using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Runtime.InteropServices;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

namespace Raymarcher{
    public class Window : GameWindow
    {
        public const int RENDER_WIDTH = 256;
        public const int RENDER_HEIGHT = 240;
        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        ShaderProgram raymarcherProgram;
        Shader raymarcherCompute;

        ShaderProgram finalProgram;

        int computeTexture;
        int emptyVAO;
        int fastRenderUniform;

        protected override void OnLoad()
        {
            //GL.Enable(EnableCap.DebugOutput);
            GL.DebugMessageCallback(MessageCallback, IntPtr.Zero);

            raymarcherCompute = new Shader("cs_raymarcher", File.ReadAllText("raymarcher.comp"), ShaderType.ComputeShader);
            raymarcherProgram = new ShaderProgram("p_raymarcher", raymarcherCompute);

            finalProgram = new ShaderProgram("p_final", new Shader("vs_final", textureRenderVertex, ShaderType.VertexShader), new Shader("fs_final", textureRenderFrag, ShaderType.FragmentShader));

            GL.CreateTextures(TextureTarget.Texture2D, 1, out computeTexture);
            GL.TextureParameter(computeTexture, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TextureParameter(computeTexture, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
            GL.TextureParameter(computeTexture, TextureParameterName.ClampToEdge, 1);
            GL.BindTexture(TextureTarget.Texture2D, computeTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, RENDER_WIDTH, RENDER_HEIGHT, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.BindImageTexture(0, computeTexture, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);

            GL.CreateVertexArrays(1, out emptyVAO);
            GL.BindVertexArray(emptyVAO);
            
            cameraPositionUniform = raymarcherProgram.GetUniform("cameraPosition");
            cameraRotationUniform = raymarcherProgram.GetUniform("cameraRotation");
            cameraMatrixUniform = raymarcherProgram.GetUniform("cameraMatrix");
            frameUniform = raymarcherProgram.GetUniform("iFrame");
            fastRenderUniform = raymarcherProgram.GetUniform("fastRender");

            base.OnLoad();
        }

        void MessageCallback( DebugSource source,
                 DebugType type,
                 int id,
                 DebugSeverity severity,
                 int length,
                 IntPtr message,
                 IntPtr userParam )
{
    if(severity == DebugSeverity.DebugSeverityHigh){
        Console.Error.WriteLine($"GL ERROR type = {type.ToString()}, severity = {severity.ToString()}, message = {Marshal.PtrToStringAuto(message)}");
    } else{
        Console.WriteLine($"GL CALLBACK: {(type == DebugType.DebugTypeError ? "**GL ERROR**" : "")} type = {type.ToString()}, severity = {severity.ToString()}, message = {Marshal.PtrToStringAuto(message)}");
    }
}


        void checkGLError()
        {
            ErrorCode err = ErrorCode.NoError;
            while((err = GL.GetError()) != ErrorCode.NoError){
                Console.WriteLine(err);
            }  
        }

        Vector3 cameraPosition = Vector3.Zero;
        int cameraPositionUniform = -1;

        Vector3 cameraRotation = Vector3.Zero;
        int cameraRotationUniform = -1;
        int cameraMatrixUniform = -1;

        float timeSinceFPSUpdate = 1f;

        int frameUniform = -1;
        int frames = 0;

        protected override void OnUpdateFrame(FrameEventArgs args)
        {

            if(IsKeyDown(Keys.W)){
                cameraPosition += (float)args.Time * Vector3.Transform(Vector3.UnitZ, Quaternion.FromEulerAngles(cameraRotation));
            }
            if(IsKeyDown(Keys.S)){
                cameraPosition -= (float)args.Time * Vector3.Transform(Vector3.UnitZ, Quaternion.FromEulerAngles(cameraRotation));
            }
            if(IsKeyDown(Keys.D)){
                cameraPosition += (float)args.Time * Vector3.Transform(Vector3.UnitX, Quaternion.FromEulerAngles(cameraRotation));
            }
            if(IsKeyDown(Keys.A)){
                cameraPosition -= (float)args.Time * Vector3.Transform(Vector3.UnitX, Quaternion.FromEulerAngles(cameraRotation));
            }
            if(IsKeyDown(Keys.LeftControl)){
                cameraPosition.Y += (float)args.Time;
            }
            if(IsKeyDown(Keys.Space)){
                cameraPosition.Y -= (float)args.Time;
            }
            if(IsKeyDown(Keys.Right)){
                cameraRotation.Y += (float)args.Time;
            }
            if(IsKeyDown(Keys.Left)){
                cameraRotation.Y -= (float)args.Time;
            }
            if(IsKeyDown(Keys.X)){
                GL.ClearTexImage(computeTexture, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            }

            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            frames++;
            if(timeSinceFPSUpdate >= 1f){
                Title = $"Jimmys raymarcher 2.0 - {MathF.Round(1f / (float)args.Time, 1)} FPS";
                timeSinceFPSUpdate = 0f;
            }
            timeSinceFPSUpdate += (float)args.Time;
            GL.Clear(ClearBufferMask.ColorBufferBit);
            
            Matrix4 cameraRotationMatrix = Matrix4.LookAt(cameraPosition, cameraPosition + Vector3.Transform(Vector3.UnitZ, Quaternion.FromEulerAngles(cameraRotation)), Vector3.UnitY);
            cameraRotationMatrix.Transpose();

            raymarcherProgram.Use();
            GL.BindImageTexture(0, computeTexture, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
            GL.Uniform3(cameraPositionUniform, cameraPosition);
            GL.UniformMatrix4(cameraRotationUniform, true, ref cameraRotationMatrix);
            GL.Uniform1(frameUniform, frames);
            GL.Uniform1(fastRenderUniform, IsKeyDown(Keys.Z) ? 1 : 0);
            GL.DispatchCompute(RENDER_WIDTH / 8, RENDER_HEIGHT / 4, 1);
            //checkGLError();
            GL.Finish();

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