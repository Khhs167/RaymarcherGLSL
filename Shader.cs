using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Raymarcher
{
    public class Shader
    {
        public int id = -1;
        public string name;

        public Shader(string name, string source, ShaderType shaderType)
        {
            this.name = name;
            id = GL.CreateShader(shaderType);
            GL.ShaderSource(id, source);
            GL.CompileShader(id);
            string log = GL.GetShaderInfoLog(id);
            if (!string.IsNullOrEmpty(log))
            {
                Console.Error.WriteLine($"Shader {name} returned a compilation error: " + log);
            }
        }

        public void Free()
        {
            GL.DeleteShader(id);
        }
    }
}
