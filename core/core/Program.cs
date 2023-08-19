
using core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.IO;

namespace prog
{
    public static class Program
    {
        internal static Color UI_COLOR = Color.BlanchedAlmond;
        internal static Color UI_ALT_COLOR = Color.DarkGray;
        public const float RAD_CONST = (float)(180 / Math.PI);
        public const float DEG_CONST = (float)(1 / (180 / Math.PI));
        public static martgame Game;

        //Internal screen size : game space
        public static Vector2 InternalScreen;

        //External screen size : what user sees
        public static Vector2 ExternalScreen;

        public static void Main(string[] args)
        {
            InternalScreen = new Vector2(1920, 1080);
            //ExternalScreen = new Vector2(1366, 768);
            ExternalScreen = new Vector2(1920, 1080) * 1.0f;

            Game = new martgame();
            
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

        private static Texture2D lineTex;
        private static bool lineTexBuilt = false;
        public static void InitDrawLine(GraphicsDevice _graphics)
        {
            if (lineTexBuilt) return;
            lineTexBuilt = true;
            lineTex = new Texture2D(_graphics, 3, 3);
            lineTex.SetData(new Color[]{
                Color.White,
                Color.White,
                Color.White,
                Color.White,
                Color.Black,
                Color.White,
                Color.White,
                Color.White,
                Color.White
            });
        }
        public static void DrawLine(GraphicsDevice _graphics, SpriteBatch _sprites, Vector2 a, Vector2 b, float thickness, bool startSprites = true)
        {
            if (startSprites)
                SpritesBeginDefault(_sprites);

            Vector2 siz = (b - a);

            float length = siz.Length();
            length = length / 3f;
            thickness = thickness / 3f;
            Vector2 scale = new Vector2(length, thickness);

            double atan = Math.Atan2(siz.Y, siz.X);

            _sprites.Draw(lineTex, a, null, Color.White, (float)atan, new Vector2(0, 1), scale, SpriteEffects.None, 0);

            if (startSprites)
                _sprites.End();
        }
    }

    public static class Input
    {
        private static KeyboardState kprev, kcurr;
        private static MouseState mprev, mcurr;

        public static void Init()
        {
            kprev = new KeyboardState();
            kcurr = new KeyboardState();

            mprev = new MouseState();
            mcurr = new MouseState();
        }

        public static void Poll(KeyboardState a, MouseState b)
        {
            kprev = kcurr;
            kcurr = a;

            mprev = mcurr;
            mcurr = b;
        }

        public static bool IsKeyPressed(Keys key)
        {
            return kcurr.IsKeyDown(key) && kprev.IsKeyUp(key);
        }
        public static bool IsKeyDown(Keys key)
        {
            return kcurr.IsKeyDown(key);
        }
        public static bool IsKeyReleased(Keys key)
        {
            return kcurr.IsKeyUp(key) && kprev.IsKeyDown(key);
        }
        public static int KeyDelta(Keys a, Keys b)
        {
            return (IsKeyDown(a) ? 1 : 0) + (IsKeyDown(b) ? -1 : 0);
        }

        public enum MouseButton
        {
            LeftMouse,
            RightMouse,
            MiddleMouse,
            X1,
            X2
        }
        public static bool IsMousePressed(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.LeftMouse:
                    return mcurr.LeftButton == ButtonState.Pressed && mprev.LeftButton == ButtonState.Released;
                case MouseButton.RightMouse:
                    return mcurr.RightButton == ButtonState.Pressed && mprev.RightButton == ButtonState.Released;
                case MouseButton.MiddleMouse:
                    return mcurr.MiddleButton == ButtonState.Pressed && mprev.MiddleButton == ButtonState.Released;
                case MouseButton.X1:
                    return mcurr.XButton1 == ButtonState.Pressed && mprev.XButton1 == ButtonState.Released;
                case MouseButton.X2:
                    return mcurr.XButton2 == ButtonState.Pressed && mprev.XButton2 == ButtonState.Released;
                default:
                    return false;
            }
        }
        public static bool IsMouseDown(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.LeftMouse:
                    return mcurr.LeftButton == ButtonState.Pressed;
                case MouseButton.RightMouse:
                    return mcurr.RightButton == ButtonState.Pressed;
                case MouseButton.MiddleMouse:
                    return mcurr.MiddleButton == ButtonState.Pressed;
                case MouseButton.X1:
                    return mcurr.XButton1 == ButtonState.Pressed;
                case MouseButton.X2:
                    return mcurr.XButton2 == ButtonState.Pressed;
                default:
                    return false;
            }
        }
        public static bool IsMouseReleased(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.LeftMouse:
                    return mcurr.LeftButton == ButtonState.Released && mprev.LeftButton == ButtonState.Pressed;
                case MouseButton.RightMouse:
                    return mcurr.RightButton == ButtonState.Released && mprev.RightButton == ButtonState.Pressed;
                case MouseButton.MiddleMouse:
                    return mcurr.MiddleButton == ButtonState.Released && mprev.MiddleButton == ButtonState.Pressed;
                case MouseButton.X1:
                    return mcurr.XButton1 == ButtonState.Released && mprev.XButton1 == ButtonState.Pressed;
                case MouseButton.X2:
                    return mcurr.XButton2 == ButtonState.Released && mprev.XButton2 == ButtonState.Pressed;
                default:
                    return false;
            }
        }
        public static int MouseDelta(MouseButton a, MouseButton b)
        {
            return (IsMouseDown(a) ? 1 : 0) + (IsMouseDown(b) ? -1 : 0);
        }

        public static Vector2 GetMousePositionRaw()
        {
            return new Vector2(mcurr.X, mcurr.Y);
        }
        public static Vector2 GetMousePosition()
        {
            Vector2 ret = GetMousePositionRaw();
            ret /= Program.ExternalScreen;
            ret *= Program.InternalScreen;

            return ret;
        }

        public static Vector2 GetMousePosDeltaRaw()
        {
            return new Vector2(mcurr.X - mprev.X, mcurr.Y - mprev.Y);
        }
        public static Vector2 GetMousePosDelta()
        {
            Vector2 ret = GetMousePosDeltaRaw();
            ret /= Program.ExternalScreen;
            ret *= Program.InternalScreen;
            return ret;
        }

        public static int GetMouseWheelDelta()
        {
            return mcurr.ScrollWheelValue - mprev.ScrollWheelValue;
        }
        public static int GetMouseWheelDeltaNormal()
        {
            int t = GetMouseWheelDelta();
            if (t > 0) return 1;
            else if (t < 0) return -1;
            return 0;
        }

        public static char GetChar(Keys key, bool shift)
        {
            if (key_reg == null)
                init_key_reg();

            for (int i = 0; i < key_reg.Length; i++)
            {
                if (key_reg[i].key_enum == (int)key)
                {
                    if (shift) return key_reg[i].key_uc;
                    return key_reg[i].key_lc;
                }
            }

            return (char)0;
        }

        private static key_reg_entry[] key_reg;
        private static void init_key_reg()
        {
            if (!File.Exists($"{Directory.GetCurrentDirectory()}\\chars.json"))
            {
                key_reg = new key_reg_entry[0];
                return;
            }
            string file = File.ReadAllText($"{Directory.GetCurrentDirectory()}\\chars.json");
            key_reg = JsonConvert.DeserializeObject<key_reg_entry[]>(file);
        }
        private static void key_reg_calibrate()
        {
            key_reg = new key_reg_entry[256];

            int count = 0;
            for (int i = 0; i < 256; i++)
            {
                if (!Enum.IsDefined(typeof(Keys), i))
                    continue;
                System.Console.WriteLine($"\n{(Keys)i}\t(lowercase / uppercase)");
                string inp = System.Console.ReadLine();
                if (inp.Equals("")) continue;
                if (inp.Equals("back"))
                {
                    i -= 2;
                    count--;
                    continue;
                }
                char[] chars = inp.ToCharArray();

                char a = chars[0];
                char b = chars[1];

                key_reg[count] = new key_reg_entry();

                key_reg[count].key_enum = i;
                key_reg[count].key_lc = a;
                key_reg[count].key_uc = b;

                System.Console.WriteLine($"{(Keys)i} => (lc){a} (uc){b}");
                count++;
            }

            key_reg_entry[] temp = new key_reg_entry[count];
            for (int i = 0; i < count; i++)
            {
                temp[i] = key_reg[i];
            }

            string serialize = JsonConvert.SerializeObject(temp);

            System.Console.Clear();
            System.Console.WriteLine(serialize);
        }
        public class key_reg_entry
        {
            public int key_enum;
            public char key_lc;
            public char key_uc;
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