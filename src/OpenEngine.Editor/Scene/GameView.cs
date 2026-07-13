// Created By Levi Enama
using OpenEngine.Core.Math;

namespace OpenEngine.Editor.Scene
{
    public class GameView
    {
        public Vector2 Resolution { get; set; }
        public bool Fullscreen { get; set; }
        public int TargetFPS { get; set; }
        public bool VSync { get; set; }
        public float AspectRatio { get; set; }

        public GameView()
        {
            Resolution = new Vector2(1920, 1080);
            Fullscreen = false;
            TargetFPS = 60;
            VSync = true;
            AspectRatio = 16f / 9f;
        }

        public void SetResolution(int width, int height)
        {
            Resolution = new Vector2(width, height);
            AspectRatio = (float)width / height;
        }

        public void SetFullscreen(bool fullscreen)
        {
            Fullscreen = fullscreen;
        }
    }
}
