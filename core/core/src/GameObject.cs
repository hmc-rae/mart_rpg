using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace core
{
    public class Camera
    {
        public Vector3 Position;
        public Vector3 TargetAngle;
        public Vector3 TargetUp = Vector3.Up;
        public Vector3 Target => Position + TargetAngle;
        public float FOV = (float)Math.PI / 2;

        internal Matrix projectionMatrix, viewMatrix, worldMatrix;
        internal BasicEffect effect;
        internal RasterizerState rasterizerState;

        private GraphicsDevice _graphics;
        private Texture2D basicTexture;
        public Camera(GraphicsDevice device)
        {
            _graphics = device;
            Position = new Vector3();
            TargetAngle = Vector3.UnitX;

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView((float)FOV, 1, 1, 1000);
            viewMatrix = Matrix.CreateLookAt(Position, Target, TargetUp);

            rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;

            effect = new BasicEffect(device);
            effect.Alpha = 1f;

            effect.VertexColorEnabled = true;
            effect.TextureEnabled = true;
            effect.LightingEnabled = false;

            basicTexture = new Texture2D(_graphics, 1, 1);
            Color[] data = { Color.Wheat };
            basicTexture.SetData(data);
        }

        /// <summary>
        /// Recompiles the projection matrix in a perspective FOV mode.
        /// </summary>
        public void Recompile(float n, float f)
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView((float)FOV, 1, n, f);
        }
        /// <summary>
        /// Recompiles the projection matrix in a orthographic mode, with the given x y parameters.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="n"></param>
        /// <param name="f"></param>
        public void Recompile(float x, float y, float n, float f)
        {
            projectionMatrix = Matrix.CreateOrthographic(x, y, n, f);
        }
        public void SetupDraw()
        {
            viewMatrix = Matrix.CreateLookAt(Position, Target, TargetUp);
            effect.Projection = projectionMatrix;
            effect.View = viewMatrix;

            _graphics.RasterizerState = rasterizerState;
        }

        public void BindWorldPosition(Vector3 pos)
        {
            worldMatrix = Matrix.CreateWorld(pos, Vector3.Forward, Vector3.Up);
            effect.World = worldMatrix;
        }

        public void BindTexture(Texture2D? tex = null)
        {
            if (tex == null)
                tex = basicTexture;
            effect.Texture = tex;
        }
    }
}
