using System;
using System.Security.Policy;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using prog;

namespace core
{
    public class Camera
    {
        public Vector3 Position;
        public Vector3 TargetAngle;
        public Vector3 TargetUp = Vector3.Up;
        public Vector3 Target => Position + TargetAngle;
        public float FOV = (float)Math.PI / 4;

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
        public void BindWorldPosition(Vector3 pos, Vector3 scale)
        {
            worldMatrix = Matrix.CreateWorld(pos, Vector3.Forward, Vector3.Up) * Matrix.CreateScale(scale);
            effect.World = worldMatrix;
        }
        public void BindWorldPosition(Vector3 pos, RotationProfile profile)
        {
            worldMatrix =   Matrix.CreateWorld(pos, Vector3.Forward, Vector3.Up) * 
                            Matrix.CreateFromAxisAngle(Vector3.UnitY, profile.ang2d.X) * Matrix.CreateFromAxisAngle(profile.left, profile.ang2d.Y);

            if (profile.roll != 0)
            {
                worldMatrix *= Matrix.CreateFromAxisAngle(profile.forward, profile.roll);
            }

            effect.World = worldMatrix;
        }

        public void BindWorldPosition(Vector3 pos, RotationProfile profile, Vector3 scale)
        {
            //Matrix.CreateFromAxisAngle(profile.left, profile.ang2d.Y) * Matrix.CreateFromAxisAngle(Vector3.UnitY, profile.ang2d.X) * Matrix.CreateFromAxisAngle(profile.forward, profile.roll);
            //Matrix.CreateFromAxisAngle(profile.forward, profile.roll) * Matrix.CreateFromAxisAngle(Vector3.UnitY, profile.ang2d.X) * Matrix.CreateFromAxisAngle(profile.left, profile.ang2d.Y);

            Matrix rotation = Matrix.CreateFromAxisAngle(profile.left, profile.ang2d.Y) * Matrix.CreateFromAxisAngle(Vector3.UnitY, profile.ang2d.X) * Matrix.CreateFromAxisAngle(profile.forward, profile.roll);

            Matrix sMatrix = Matrix.CreateScale(scale);

            worldMatrix = sMatrix;
            //worldMatrix *= rotation;
            worldMatrix *= Matrix.CreateWorld(pos, profile.forward, Vector3.Up);

            //System.Console.WriteLine

            effect.World = worldMatrix;
        }

        public void BindTexture(Texture2D? tex = null)
        {
            if (tex == null)
                tex = basicTexture;
            effect.Texture = tex;
        }
    }
    public class Rotor
    {
        public const float AZIMUTH_MAX = (float)Math.PI;

        public float ELEVATION_MAX;
        public float Multiplier;

        private float azi, ele;
        private Vector2 azi_v, ele_v;

        private Vector3 ang;
        public float Azimuth => azi;
        public float Elevation => ele;
        public Vector2 Azimuth_V => azi_v;
        public Vector2 Elevation_V => ele_v;
        public Vector2 Angle2D => new Vector2(azi, ele);
        public Vector3 Angle => ang;

        private RotationProfile prof;
        public RotationProfile Profile => prof;

        public Rotor(float emax = (float)Math.PI / 2.125f)
        {
            azi = 0;
            ele = 0;
            azi_v = Vector2.UnitX;
            ang = Vector3.UnitX;
            ele_v = Vector2.UnitX;
            ELEVATION_MAX = emax;

            prof = new RotationProfile();

            push();

            Multiplier = 1;
        }

        public void Update(Vector2 delta)
        {
            Update(delta.X, delta.Y);
        }
        public void Update(float aD, float eD)
        {
            azi = (azi + (Multiplier * aD));
            ele = (ele + (Multiplier * eD));

            if (ele < -ELEVATION_MAX) ele = -ELEVATION_MAX;
            if (ele > ELEVATION_MAX) ele = ELEVATION_MAX;
            push();
        }
        public void Set(Vector2 a)
        {
            Set(a.X, a.Y);
        }
        public void Set(float a, float e)
        {
            azi = (Multiplier * a) % AZIMUTH_MAX;
            ele = (Multiplier * e) % ELEVATION_MAX;
            push();
        }

        private void push()
        {
            azi_v = ToVec2(azi);
            ele_v = ToVec2(ele);

            prof.ang2d = new Vector2(azi, ele);
            prof.left = new Vector3(azi_v.Y, 0, -azi_v.X);

            prof.forward = ang = new Vector3(azi_v.X * ele_v.X, ele_v.Y, azi_v.Y * ele_v.X);
        }
        public static Vector3 GetLeftUNFlat(Vector3 angl)
        {
            Vector3 left = (angl * new Vector3(1, 0, 1));
            return new Vector3(left.Y, 0, -left.X);
        }
        public static Vector3 GetElevationFlat(Vector3 angl)
        {
            Vector3 flatL = GetLeftUNFlat(angl);
            return new Vector3(flatL.Length(), angl.Y, 0);
        }
        public static Vector3 GetElevationFlat(Vector3 angl, Vector3 flatL)
        {
            return new Vector3(angl.Y, flatL.Length(), 0);
        }

        public static Vector3 GetLeft(Vector3 angl)
        {
            Vector3 lf = GetLeftUNFlat(angl);
            Vector3 uf = GetElevationFlat(angl, lf);
            return new Vector3(lf.X * uf.X, uf.Y, lf.Y * uf.X);
        }

        public static Vector3 GetLeft(Vector3 angl, Vector3 lf)
        {
            Vector3 uf = GetElevationFlat(angl, lf);
            return new Vector3(lf.X * uf.X, uf.Y, lf.Y * uf.X);
        }
        public static Vector2 ToVec2(float a)
        {
            return new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
        }
        public static float ToFloat(Vector2 a)
        {
            return (float)Math.Atan2(a.Y, a.X);
        }
    }
    public struct RotationProfile
    {
        public Vector3 left;
        public Vector3 forward;

        public Vector2 ang2d;
        public float roll;

        public RotationProfile()
        {
            forward = Vector3.UnitX;
            left = Vector3.UnitZ;

            ang2d = Vector2.Zero;
            roll = 0;
        }

        public RotationProfile(Vector3 angl)
        {
            forward = angl;
            left = (angl * new Vector3(1, 0, 1));
            left.Normalize();
            roll = 0;

            float elev = (float)Math.Asin(forward.Y);
            float azi = (float)Math.Asin(left.Z);

            left = new Vector3(left.Y, 0, -left.X);

            ang2d = new Vector2(azi, elev);
        }

        public RotationProfile(float azi, float ele, float rol)
        {
            ang2d = new Vector2(azi, ele);
            roll = rol;

            Vector2 tmpA = Rotor.ToVec2(azi);
            Vector2 tmpB = Rotor.ToVec2(ele);

            forward = new Vector3(tmpA.X * tmpB.X, tmpB.Y, tmpA.Y * tmpB.X);
            left = new Vector3(tmpA.Y, 0, -tmpA.X);
        }

    }
}
