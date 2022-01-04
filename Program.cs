using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Raymarcher
{
    public static class Program{
        public static void Main(string[] args){
            NativeWindowSettings nws = new NativeWindowSettings();
            nws.API = ContextAPI.OpenGL;
            nws.APIVersion = new Version(4, 3);
            nws.Title = "Jimmys raymarcher 2.0";
            
            GameWindowSettings gws = new GameWindowSettings();
            gws.RenderFrequency = 60;
            gws.UpdateFrequency = 60;

            Window window = new Window(gws, nws);
            window.Run();
        }
    }
}