
using core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace prog
{
    public static class Program
    {
        public static imsimgame Game;
        public static void Main(string[] args)
        {
            Game = new imsimgame();
            Game.Run();
        }

        public static void SpritesBeginDefault(SpriteBatch sprites)
        {
            sprites.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.Default,
                RasterizerState.CullNone);
        }
        public static string Vec3ToString(Vector3 v)
        {
            return $"{String.Format("{0:0.00}", v.X)}, {String.Format("{0:0.00}", v.Y)}, {String.Format("{0:0.00}", v.Z)}";
        }
    }

    public interface ResourceInterface
    {
        public abstract void Poll();
        public abstract void PreRender();
        public abstract void Render(bool beginSprites = true);
        public abstract void Destroy();
    }
}