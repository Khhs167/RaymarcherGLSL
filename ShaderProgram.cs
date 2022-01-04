using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Raymarcher
{
    public class ShaderProgram
    {
        public int id = -1;
        public string name;

        public List<Shader> shaders;

        public void Use(){
            GL.UseProgram(id);
        }

        public ShaderProgram(string name, params Shader[] shaders)
        {
            this.name = name;
            id = GL.CreateProgram();
            for (int i = 0; i < shaders.Length; i++)
            {
                GL.AttachShader(id, shaders[i].id);
            }
            GL.LinkProgram(id);

            string log = GL.GetProgramInfoLog(id);
            if (!string.IsNullOrEmpty(log))
            {
                Console.Error.WriteLine($"ShaderProgram {name} returned a compilation error: " + log);
            }

            for (int i = 0; i < shaders.Length; i++)
            {
                GL.DetachShader(id, shaders[i].id);
            }

            this.shaders = shaders.ToList();

        }

        public void Free()
        {
            GL.DeleteProgram(id);
        }

        private static readonly string DefaultVertexShader = @"
#version 330 core
layout (location = 0) in vec3 aPosition;

uniform float x;
uniform float y;

uniform float width;
uniform float height;

uniform int swidth;
uniform int sheight;

uniform ivec2 origin;

out vec2 uv;

void main()
{
    float xPos = ((aPosition.x * width  + x ) - (width / origin.x)) / (float(swidth) / 2) - 1.0;
    float yPos = ((aPosition.y * height - y) - (height / origin.y)) / (float(sheight) / 2) + 1.0;
    gl_Position = vec4(xPos, yPos, aPosition.z, 1.0);
    uv = vec2(aPosition.x, aPosition.y);
}";
        private static readonly string DefaultFragmentShader = @"
#version 330 core
out vec4 FragColor;

in vec2 uv;

uniform sampler2D texture0;

void main()
{
    FragColor = texture(texture0, uv);
    //FragColor = vec4(1.0f, 1.0f, 1.0f, 1.0f);
}";

        public static readonly ShaderProgram DefaultShader = new ShaderProgram("Default", new Shader("Default Vertex Shader", DefaultVertexShader, ShaderType.VertexShader), new Shader("Default Fragment Shader", DefaultFragmentShader, ShaderType.FragmentShader));
    }
}
