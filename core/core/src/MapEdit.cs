using Microsoft.VisualBasic.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using prog;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;

namespace core.src
{
    public class MapEdit : GameScene
    {
        ContentManager _content;

        public PerspectiveViewer curViewer;
        public BrushLibrary library;
        public override void OnCreate()
        {
            base.OnCreate();
        }
        public override void Load(ContentManager content)
        {
            base.OnDestroy();

            _content = content;
            library = new BrushLibrary(this);
            curViewer = new BrushEditor(this, library);
        }

        public override void OnFrame()
        {
            curViewer.prepoll();
        }
        public override void PreRender()
        {
            curViewer.prerender();
        }
        public override void OnRender()
        {
            curViewer.render();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            curViewer.Destroy();
        }
    }
    
    public class VertexModel
    {
        public static int COMPILE_ROTATION = 1;
        public static int COMPILE_POLYGONS = 2;
        private GraphicsDevice _device;
        public Vector3 position;

        public Vector3 rotation_x, rotation_y, rotation_z;

        private Matrix rotmat;

        public TexturedPolygon[] polygons;   
        public class TexturedPolygon
        {
            public VertexPositionColorTexture[] verticies;
            public VertexBuffer buffer;
            public int primitiveCount;
            public PrimitiveType primitiveType;

            public bool built;

            private GraphicsDevice _device;

            private static int id_master = 0;
            private int id;

            public TexturedPolygon(GraphicsDevice device, int vertexcount, int primitives, PrimitiveType type)
            {
                id = id_master++;
                built = false;
                _device = device;

                verticies = new VertexPositionColorTexture[vertexcount];
                primitiveCount = primitives;
                primitiveType = type;


                buffer = new VertexBuffer(_device, typeof(VertexPositionColorTexture), verticies.Length, BufferUsage.WriteOnly);
            }

            public void Recompile()
            {
                buffer.SetData<VertexPositionColorTexture>(verticies);
                built = true;
            }

            public override string ToString()
            {
                return $"Polygon#{id}";
            }
        }

        public VertexModel(GraphicsDevice device, int pc)
        {
            _device = device;
            position = Vector3.Zero;
            polygons = new TexturedPolygon[pc];

            rotation_x = Vector3.UnitX;
            rotation_y = Vector3.UnitY;
            rotation_z = Vector3.UnitZ;
        }
        public void Recompile(int mode)
        {
            if ((mode & COMPILE_ROTATION) != 0)
            {
                //Recomp rotation vectors to a rotation matrix
            }

            if ((mode & COMPILE_POLYGONS) != 0)
            {
                //Recomp all polygons
                for (int i = 0; i < polygons.Length; i++)
                {
                    polygons[i].Recompile();
                }
            }
        }
        public void Draw(Camera camera)
        {
            camera.BindTexture();
            camera.BindWorldPosition(position);
            for (int n = 0; n < polygons.Length; n++)
            {
                if (!polygons[n].built) continue;
                _device.SetVertexBuffer(polygons[n].buffer);
                foreach (EffectPass pass in camera.effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _device.DrawPrimitives(polygons[n].primitiveType, 0, polygons[n].primitiveCount);
                }
            }
        }
        public void Draw(Camera camera, int n)
        {
            camera.BindTexture();
            camera.BindWorldPosition(position);
            if (n < 0 || n >= polygons.Length) return;

            if (!polygons[n].built) return;
            _device.SetVertexBuffer(polygons[n].buffer);
            foreach (EffectPass pass in camera.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _device.DrawPrimitives(polygons[n].primitiveType, 0, polygons[n].primitiveCount);
            }
        }

        public static VertexModel CreateGeneric(GraphicsDevice device)
        {
            VertexModel temp = new VertexModel(device, 6);

            //0: heading +x, yz
            temp.polygons[0] = new TexturedPolygon(device, 4, 2, PrimitiveType.TriangleStrip);
            temp.polygons[0].verticies = Generate4Point(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ);

            //1: heading -x, yz
            temp.polygons[1] = new TexturedPolygon(device, 4, 2, PrimitiveType.TriangleStrip);
            temp.polygons[1].verticies = Generate4Point(Vector3.UnitX * -1, Vector3.UnitY, Vector3.UnitZ);

            //2: heading +y, xz
            temp.polygons[2] = new TexturedPolygon(device, 4, 2, PrimitiveType.TriangleStrip);
            temp.polygons[2].verticies = Generate4Point(Vector3.UnitY, Vector3.UnitX, Vector3.UnitZ);

            //3: heading -y, xz
            temp.polygons[3] = new TexturedPolygon(device, 4, 2, PrimitiveType.TriangleStrip);
            temp.polygons[3].verticies = Generate4Point(Vector3.UnitY * -1, Vector3.UnitX, Vector3.UnitZ);

            //4: heading +z, xy
            temp.polygons[4] = new TexturedPolygon(device, 4, 2, PrimitiveType.TriangleStrip);
            temp.polygons[4].verticies = Generate4Point(Vector3.UnitZ, Vector3.UnitX, Vector3.UnitY);

            //5: heading -z, xy
            temp.polygons[5] = new TexturedPolygon(device, 4, 2, PrimitiveType.TriangleStrip);
            temp.polygons[5].verticies = Generate4Point(Vector3.UnitZ * -1, Vector3.UnitX, Vector3.UnitY);

            temp.rotation_x = Vector3.UnitX;
            temp.rotation_y = Vector3.UnitY;
            temp.rotation_z = Vector3.UnitZ;

            temp.Recompile(COMPILE_POLYGONS);

            return temp;
        }
        internal static VertexPositionColorTexture[] Generate4Point(Vector3 dir, Vector3 a, Vector3 b)
        {
            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[4];

            verts[0] = new VertexPositionColorTexture(dir + ( a) + ( b), ColorByVectors(dir, a, b), new Vector2(0, 0));
            verts[1] = new VertexPositionColorTexture(dir + (-a) + ( b), ColorByVectors(dir, -a, b), new Vector2(1, 0));
            verts[2] = new VertexPositionColorTexture(dir + ( a) + (-b), ColorByVectors(dir, a, -b), new Vector2(0, 1));
            verts[3] = new VertexPositionColorTexture(dir + (-a) + (-b), ColorByVectors(dir, -a, -b), new Vector2(1, 1));

            return verts;
        }
        internal static VertexPositionColorTexture[] Generate4Point(Vector3 dir, Vector3 a, Vector3 b, Color over)
        {
            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[4];

            verts[0] = new VertexPositionColorTexture(dir + (a) + (b), over, new Vector2(0, 0));
            verts[1] = new VertexPositionColorTexture(dir + (-a) + (b), over, new Vector2(1, 0));
            verts[2] = new VertexPositionColorTexture(dir + (a) + (-b), over, new Vector2(0, 1));
            verts[3] = new VertexPositionColorTexture(dir + (-a) + (-b), over, new Vector2(1, 1));

            return verts;
        }
        internal static Color ColorByVectors(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 sum = a + b + c;
            //sum += Vector3.One;
            //sum /= 2;
            return new Color(sum);
        }
    }

    public class ButtonWidget2D
    {
        public enum ButtonMode
        {
            Hold,
            Toggle,
            Attend
        }
        public ButtonMode mode;
        public Vector2 screenPos;
        public Vector2 dimensions;

        private imsimgame _game;
        private GraphicsDevice _graphics;
        private Texture2D _rect;

        private bool bdown;
        public bool ButtonDown => bdown;

        public Vector2 multscale;
        public Vector2 offset;

        public ButtonWidget2D(ButtonMode mode, Vector2 screenPos, Vector2 dimensions)
        {
            _game = Program.Game;
            _graphics = _game.GraphicsDevice;

            this.mode = mode;
            this.screenPos = screenPos;
            this.dimensions = dimensions;
            bdown = false;

            _rect = new Texture2D(_graphics, (int)dimensions.X, (int)dimensions.Y);
            Color[] data = new Color[(int)(dimensions.X) * (int)(dimensions.Y)];
            for (int i = 0; i < data.Length; i++) data[i] = Color.Gray;

            _rect.SetData(data);

            multscale = Vector2.One;
            offset = Vector2.Zero;
        }

        public void Recompile()
        {
            _rect = new Texture2D(_graphics, (int)dimensions.X, (int)dimensions.Y);
            Color[] data = new Color[(int)(dimensions.X) * (int)(dimensions.Y)];
            for (int i = 0; i < data.Length; i++) data[i] = Color.Gray;

            _rect.SetData(data);
        }

        public void Poll()
        {
            Vector2 msps = _game.CurrentMouse.Position.ToVector2() - offset;
            msps *= multscale;

            msps = msps - screenPos;
            bool insideMouse = (msps.X <= dimensions.X && msps.Y <= dimensions.Y && msps.X >= 0 && msps.Y >= 0);

            switch (mode)
            {
                case ButtonMode.Toggle:
                    if (!insideMouse) break;

                    if (_game.CurrentMouse.LeftButton == ButtonState.Pressed && _game.PreviousMouse.LeftButton == ButtonState.Released)
                    {
                        bdown = !bdown;
                    } 
                    break;
                case ButtonMode.Hold:

                    if (!insideMouse)
                    {
                        bdown = false;
                        break;
                    }

                    if (!ButtonDown)
                    {
                        if (_game.CurrentMouse.LeftButton == ButtonState.Pressed && _game.PreviousMouse.LeftButton == ButtonState.Released)
                        {
                            bdown = true;
                        }
                    }
                    else
                    {
                        if (_game.CurrentMouse.LeftButton != ButtonState.Pressed)
                        {
                            bdown = false;
                        }
                    }

                    break;

                case ButtonMode.Attend:
                    if (!insideMouse)
                    {
                        if (_game.CurrentMouse.LeftButton == ButtonState.Pressed && _game.PreviousMouse.LeftButton == ButtonState.Released)
                            bdown = false;
                    }
                    else 
                    {
                        if (_game.CurrentMouse.LeftButton == ButtonState.Pressed && _game.PreviousMouse.LeftButton == ButtonState.Released)
                            bdown = true;
                    }
                    break;
            }
        }

        public void Destroy()
        {
            _rect.Dispose();
        }
    }
    public class ListViewer<T> : ResourceInterface
    {
        static int spacing = 20;

        public List<T> viewedList;

        public int curIndex;
        public int scrolledOffset;
        public int pageSize;

        public Vector2 position, size;
        private Texture2D _rect, _rectSel;
        private ButtonWidget2D _button;
        private SpriteFont _font;
        private SpriteBatch _sprites;
        private ContentManager _content;

        public ListViewer(List<T> list, int sizeOfPage, Vector2 p, Vector2 s, ContentManager content)
        {
            _content = content;
            viewedList = list;
            pageSize = sizeOfPage;

            _sprites = Program.Game._spriteBatch;
            curIndex = -1;
            scrolledOffset = 0;

            position = p;
            size = s;

            _button = new ButtonWidget2D(ButtonWidget2D.ButtonMode.Attend, position, size);

            _rect = new Texture2D(Program.Game.GraphicsDevice, (int)s.X, (int)s.Y);

            Color[] data = new Color[(int)s.X * (int)s.Y];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.LightGray;
            }

            _rect.SetData(data);

            _rectSel = new Texture2D(Program.Game.GraphicsDevice, (int)s.X - 20, 20);
            data = new Color[(int)(s.X - 20) * 20];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.LightCoral;
            }
            _rectSel.SetData(data);

            _font = content.Load<SpriteFont>("Fonts\\Arial16");
        }

        public void Poll()
        {
            if (!_button.ButtonDown)
            {
                _button.Poll();
                return;
            }
            _button.Poll();

            if (!_button.ButtonDown) return;

            int scroll = Program.Game.CurrentMouse.ScrollWheelValue - Program.Game.PreviousMouse.ScrollWheelValue;
            if (scroll > 0) scroll = 1;
            if (scroll < 0) scroll = -1;

            scroll = -scroll;
            //if (scroll != 0) System.Console.WriteLine($"Scroll: {scroll}");
            scrolledOffset += scroll;
            if (scrolledOffset >= viewedList.Count - pageSize) scrolledOffset = (viewedList.Count - pageSize) - 1;
            if (scrolledOffset < 0) scrolledOffset = 0;

            bool mdown = Program.Game.CurrentMouse.LeftButton == ButtonState.Pressed && Program.Game.PreviousMouse.LeftButton == ButtonState.Released;
            Vector2 pos = Program.Game.CurrentMouse.Position.ToVector2() - position;

            if (mdown && pos.X <= size.X)
            {
                int idx = (int)pos.Y / 20;

                idx += scrolledOffset;
                if (idx >= 0 && idx < viewedList.Count)
                    curIndex = idx;
                else 
                {
                    curIndex = -1;
                }
            }
        }

        public T? GetSelectedItem()
        {
            if (curIndex < 0 && curIndex >= viewedList.Count) return default;

            return viewedList[curIndex];
        }

        public void PreRender() { }
        public void Render(bool beginSprites = true)
        {
            if (beginSprites)
                _sprites.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.Default,
                RasterizerState.CullNone);

            _sprites.Draw(_rect, position, Color.LightGray);

            Vector2 pos = position;
            pos.X += 5;
            pos.Y += 5;

            for (int K = scrolledOffset, N = 0; N < pageSize && K >= 0 && K < viewedList.Count; N++, K++)
            {
                if (K == curIndex)
                {
                    _sprites.Draw(_rectSel, pos, Color.LightCoral);
                }
                _sprites.DrawString(_font, viewedList[K].ToString(), pos, Color.Black);

                pos.Y += spacing;
            }

            if (beginSprites)
                _sprites.End();
        }

        public void Destroy()
        {
            _rect.Dispose();
            _button.Destroy();
        }
    }
    public class Brush
    {
        private int id;
        private static int id_const;
        public int ID => id;

        public VertexModel physical;

        public List<VertexModel.TexturedPolygon> modelPolys;

        //Add all details here

        public Brush()
        {
            id = id_const++;
            modelPolys = new List<VertexModel.TexturedPolygon>();
        }

        public void Bind(VertexModel mod)
        {
            modelPolys.Clear();
            for (int i = 0; i < mod.polygons.Length; i++)
            {
                modelPolys.Add(mod.polygons[i]);
            }
            physical = mod;
        }

        /// <summary>
        /// Updates the physical model to be equivalent 
        /// </summary>
        public void Correct()
        {

        }

        public override string ToString()
        {
            return $"{ID}";
        }
    }
    public class BrushLibrary : ResourceInterface
    {
        public List<Brush> brushes;

        public ListViewer<Brush> brushViewer;

        public ButtonWidget2D button;

        private GameScene _parent;
        private GraphicsDevice _graphics;
        private ContentManager _content;
        private Texture2D _rect;
        private SpriteBatch _sprites;

        public BrushLibrary(GameScene parent)
        {
            _parent = parent;
            _graphics = parent._game.GraphicsDevice;
            _content = parent._game.Content;
            _sprites = parent._game._spriteBatch;

            brushes = new List<Brush>();
            Vector2 pos = new Vector2(_graphics.PresentationParameters.BackBufferHeight, 32);
            pos.X += ((_graphics.PresentationParameters.BackBufferWidth - _graphics.PresentationParameters.BackBufferHeight) / 3f) * 2;

            Vector2 size = new Vector2((_graphics.PresentationParameters.BackBufferWidth - _graphics.PresentationParameters.BackBufferHeight) / 3f, _graphics.PresentationParameters.BackBufferHeight);
            brushViewer = new ListViewer<Brush>(brushes, 32, pos, size, _content);

            pos.Y = 0;
            button = new ButtonWidget2D(ButtonWidget2D.ButtonMode.Attend, pos, size);

            size.Y = 32;

            _rect = new Texture2D(_graphics, (int)size.X, (int)size.Y);

            Color[] data = new Color[(int)size.X * (int)size.Y];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Gray;
            }

            _rect.SetData(data);
        }

        public void Poll()
        {
            button.Poll();
            brushViewer.Poll();

            if (button.ButtonDown)
            {
                if (Program.Game.CurrentKeyboard.IsKeyDown(Keys.N) && Program.Game.PreviousKeyboard.IsKeyUp(Keys.N)) //generate fresh cuboid brush
                {
                    VertexModel temp = VertexModel.CreateGeneric(_graphics);
                    Brush brush = new Brush();
                    brush.Bind(temp);

                    brushes.Add(brush);
                }
            }
        }

        public void PreRender() { }
        public void Render(bool beginSprites = true)
        {
            if (beginSprites)
                _sprites.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.Default,
                RasterizerState.CullNone);

            _sprites.Draw(_rect, button.screenPos, Color.Gray);

            brushViewer.Render(false);

            if (beginSprites)
                _sprites.End();
        }
        public void Destroy()
        {
            _rect.Dispose();
            brushViewer.Destroy();
        }
    }

    public class PerspectiveViewer
    {
        public static int PERSPECTIVE_ID = 0;
        public static int BIRDSEYE_ID = 1;
        public static int FORWARD_ID = 2;
        public static int SIDE_ID = 3;

        protected GraphicsDevice _graphics;
        protected GameScene _parent;
        protected SpriteBatch _sprites;
        protected SpriteFont font;
        internal static DepthStencilState _dss = new DepthStencilState() { DepthBufferEnable = true, DepthBufferFunction = CompareFunction.Less };

        //RENDER TARGET DEETS
        public class PerspectiveViewerWindow
        {
            public int ID;
            public RenderTarget2D Target;
            public Rectangle Culler;
            public Vector2 TargetOrigin;
            public ButtonWidget2D Focuser;
            public Camera Camera;
            public float Range, Aim;

            public Vector3 Local_X, Local_Y, Local_Z, Position;

            private GraphicsDevice _graphics;
            private GameScene _parent;
            private SpriteBatch _sprites;

            public PerspectiveViewerWindow(GameScene parent, int id, Vector2 origin, Vector2 dimensions)
            {
                _parent = parent;
                _graphics = parent._game.GraphicsDevice;
                _sprites = parent._game._spriteBatch;

                ID = id;

                TargetOrigin = origin;
                Target = new RenderTarget2D(_graphics, (int)dimensions.X, (int)dimensions.Y, true, SurfaceFormat.Alpha8, DepthFormat.Depth24Stencil8);

                Culler = new Rectangle(0, 0, (int)dimensions.X, (int)dimensions.Y);

                Focuser = new ButtonWidget2D(ButtonWidget2D.ButtonMode.Attend, origin, dimensions);
                
                Camera = new Camera(_graphics);

                Position = Vector3.Zero;

                Aim = 0;
                Range = 1;
            }

            bool ortho = false;
            public void BuildPerspective(float n, float f)
            {
                Local_X = Vector3.UnitX;
                Local_Y = Vector3.UnitY;
                Local_Z = Vector3.UnitZ;

                Camera.Recompile(n, f);

                ortho = false;
            }

            public void BuildOrthographic(Vector3 f, Vector3 u, Vector3 r)
            {
                Local_X = r;
                Local_Z = f;
                Local_Y = u;

                Camera.TargetUp = Local_Y;
                Camera.TargetAngle = Local_Z;
                Camera.Position = Position + (Local_Z * Aim);

                Camera.Recompile(10, 10, -Range, Range);

                ortho = true;
            }

            public void Poll()
            {
                Focuser.Poll();
                if (ortho)
                {
                    Camera.Position = Position + (Local_Z * Aim);
                    Camera.Recompile(10, 10, -Range, Range);
                }
            }

            public void Destroy()
            {
                Target.Dispose();
                Focuser.Destroy();
            }
        }
        protected int _targetID;

        protected PerspectiveViewerWindow[] windows;
        protected string[] names = { "Perspective", "Top", "Front", "Side" };
        protected Color[] colors =
        {
            Color.White,
            Color.Red,
            Color.Green,
            Color.Blue
        };

        public PerspectiveViewer(GameScene parent)
        {
            _parent = parent;
            _graphics = parent._game.GraphicsDevice;
            _sprites = _parent._game._spriteBatch;

            _targetID = -1;

            windows = new PerspectiveViewerWindow[4];
            Vector2 size = new Vector2(_graphics.PresentationParameters.BackBufferHeight / 2, _graphics.PresentationParameters.BackBufferHeight / 2);
            for (int i = 0; i < windows.Length; i++)
            {
                Vector2 temp = new Vector2(i % 2, i / 2);
                temp *= _graphics.PresentationParameters.BackBufferHeight / 2;

                windows[i] = new PerspectiveViewerWindow(_parent, i, temp, size);
            }

            //Setup cameras to be appropriate

            //Cam 0 is just normal 3d space camera
            windows[0].BuildPerspective(0.1f, 100);

            //Cam 1 will be birds eye with the up angle being forward, point to the right
            windows[1].BuildOrthographic(Vector3.Down, Vector3.Forward, Vector3.Right);

            //Cam 2 will be forwards
            windows[2].BuildOrthographic(Vector3.Forward, Vector3.Up, Vector3.Right);

            //Cam 3 will be sideways
            windows[3].BuildOrthographic(Vector3.Left, Vector3.Up, Vector3.Forward);
        }
        public void prepoll()
        {
            for (int i = 0; i < 4; i++)
            {
                windows[i].Poll();
                if (windows[i].Focuser.ButtonDown)
                {
                    _targetID = i;
                }
            }

            Poll();

            if (windows[PERSPECTIVE_ID].Focuser.ButtonDown)
                DefaultProcess(windows[PERSPECTIVE_ID]);

            if (windows[BIRDSEYE_ID].Focuser.ButtonDown)
                OrthoProcess(windows[BIRDSEYE_ID], WindowContext.Birdseye);

            if (windows[FORWARD_ID].Focuser.ButtonDown)
                OrthoProcess(windows[FORWARD_ID], WindowContext.Forward);

            if (windows[SIDE_ID].Focuser.ButtonDown)
                OrthoProcess(windows[SIDE_ID], WindowContext.Side);
        }
        public virtual void Poll() { }
        public void prerender()
        {
            _graphics.SetRenderTarget(windows[PERSPECTIVE_ID].Target);
            DefaultRender(windows[PERSPECTIVE_ID]);

            for (int i = BIRDSEYE_ID; i <= SIDE_ID; i++)
            {
                _graphics.SetRenderTarget(windows[i].Target);
                OrthoRender(windows[i], (WindowContext)i);
            }
        }
        public void render()
        {
            _sprites.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.Default,
                RasterizerState.CullNone);

            for (int i = 0; i < 4; i++)
            {
                _sprites.Draw(windows[i].Target, windows[i].TargetOrigin, windows[i].Culler, Color.White);
            }

            _sprites.End();

            AddlRender();
        }
        public virtual void AddlRender() { }

        //Default perspective mode
        public virtual void DefaultProcess(PerspectiveViewerWindow window) { }
        public virtual void DefaultRender(PerspectiveViewerWindow window) { }

        //Orthographic process (will apply to all 3 windows)
        public enum WindowContext
        {
            Perspective = 0,
            Birdseye = 1,
            Forward = 2,
            Side = 3
        }
        public virtual void OrthoProcess(PerspectiveViewerWindow window, WindowContext context) 
        {
            Vector3 Y = ((Program.Game.CurrentKeyboard.IsKeyDown(Keys.W) ? 1 : 0) - (Program.Game.CurrentKeyboard.IsKeyDown(Keys.S) ? 1 : 0)) * window.Local_Y;
            Vector3 X = ((Program.Game.CurrentKeyboard.IsKeyDown(Keys.D) ? 1 : 0) - (Program.Game.CurrentKeyboard.IsKeyDown(Keys.A) ? 1 : 0)) * window.Local_X;

            Y *= (float)(Program.Game.FrameTime * 5);
            X *= (float)(Program.Game.FrameTime * 5);

            window.Position += Y + X;

            int scroll = Program.Game.PreviousMouse.ScrollWheelValue - Program.Game.CurrentMouse.ScrollWheelValue;
            if (scroll > 0) scroll = 1;
            if (scroll < 0) scroll = -1;

            window.Aim += (float)(scroll / Program.Game.FrameRate) * 10;

            int rangeDelta = ((Program.Game.CurrentKeyboard.IsKeyDown(Keys.Q) ? 1 : 0) - (Program.Game.CurrentKeyboard.IsKeyDown(Keys.E) ? 1 : 0));

            window.Range += (float)(rangeDelta / Program.Game.FrameRate);
            if (window.Range < 0) window.Range = 0;
        }
        public virtual void OrthoRender(PerspectiveViewerWindow window, WindowContext context) 
        {
            Program.SpritesBeginDefault(_sprites);

            _sprites.DrawString(font, names[window.ID], Vector2.Zero, colors[window.ID]);
            _sprites.DrawString(font, $"({Program.Vec3ToString(window.Camera.Position)})", new Vector2(0, 20), Color.White);
            _sprites.DrawString(font, $"{String.Format("{0:0.00}", window.Range)}", new Vector2(0, 40), Color.White);

            _sprites.End();
        }

        public virtual void Destroy()
        {
            for (int i = 0; i < 4; i++)
            {
                windows[i].Destroy();
            }
        }
    }
    public class BrushEditor : PerspectiveViewer
    {
        public Brush CurrentBrush;
        public BrushLibrary library;


        protected ListViewer<VertexModel.TexturedPolygon> brushContentsList;

        protected VertexModel[] xyzPlanes;
        private bool renderPlanes;
        public BrushEditor(GameScene parent, BrushLibrary lib) : base(parent)
        {
            library = lib;
            Vector2 p = new Vector2(_graphics.PresentationParameters.BackBufferHeight, 0);
            Vector2 s = new Vector2((_graphics.PresentationParameters.BackBufferWidth - _graphics.PresentationParameters.BackBufferHeight) / 3, _graphics.PresentationParameters.BackBufferHeight / 2);
            
            brushContentsList = new ListViewer<VertexModel.TexturedPolygon>(new List<VertexModel.TexturedPolygon>(), 20, p, s, Program.Game.Content);
            CurrentBrush = null;

            windows[PERSPECTIVE_ID].Camera.TargetAngle = new Vector3(-1, -1, -1);
            windows[PERSPECTIVE_ID].Camera.Position = windows[PERSPECTIVE_ID].Camera.TargetAngle * -d_Distance;
            font = Program.Game.Content.Load<SpriteFont>("Fonts\\Arial16");

            xyzPlanes = new VertexModel[4];

            int opac = 64;
            
            //X facing (SIDE)
            xyzPlanes[SIDE_ID] = new VertexModel(_graphics, 1);
            xyzPlanes[SIDE_ID].polygons[0] = new VertexModel.TexturedPolygon(_graphics, 4, 2, PrimitiveType.TriangleStrip);
            xyzPlanes[SIDE_ID].polygons[0].verticies = VertexModel.Generate4Point(Vector3.Zero, Vector3.UnitY * 5, Vector3.UnitZ * 5, new Color(colors[SIDE_ID], opac));

            //Z facing (FORWARD)
            xyzPlanes[FORWARD_ID] = new VertexModel(_graphics, 1);
            xyzPlanes[FORWARD_ID].polygons[0] = new VertexModel.TexturedPolygon(_graphics, 4, 2, PrimitiveType.TriangleStrip);
            xyzPlanes[FORWARD_ID].polygons[0].verticies = VertexModel.Generate4Point(Vector3.Zero, Vector3.UnitY * 5, Vector3.UnitX * 5, new Color(colors[FORWARD_ID], opac));

            //Y facing (TOP)
            xyzPlanes[BIRDSEYE_ID] = new VertexModel(_graphics, 1);
            xyzPlanes[BIRDSEYE_ID].polygons[0] = new VertexModel.TexturedPolygon(_graphics, 4, 2, PrimitiveType.TriangleStrip);
            xyzPlanes[BIRDSEYE_ID].polygons[0].verticies = VertexModel.Generate4Point(Vector3.Zero, Vector3.UnitZ * 5, Vector3.UnitX * 5, new Color(colors[BIRDSEYE_ID], opac));

            for (int i = 1; i < 4; i++)
            {
                xyzPlanes[i].Recompile(VertexModel.COMPILE_POLYGONS);
                xyzPlanes[i].position = windows[i].Camera.Position;
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            brushContentsList.Destroy();
        }

        int lastIndexLib = -1;
        public override void Poll()
        {
            brushContentsList.Poll();
            library.Poll();

            if (lastIndexLib != library.brushViewer.curIndex)
            {
                lastIndexLib = library.brushViewer.curIndex;
                if (lastIndexLib != -1)
                {
                    CurrentBrush = library.brushViewer.viewedList[lastIndexLib]; 
                    brushContentsList.viewedList = CurrentBrush.modelPolys;
                    brushContentsList.curIndex = -1;
                }
                else
                {
                    CurrentBrush = null;
                    brushContentsList.viewedList = new List<VertexModel.TexturedPolygon>();
                    brushContentsList.curIndex = -1;
                }
            }

            //Get idx of the list and select the appropriate texturedpolygon. 
        }
        public override void AddlRender()
        {
            _sprites.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.Default,
                RasterizerState.CullNone);
            
            brushContentsList.Render(false);

            library.Render(false);

            _sprites.End();
        }

        //DEFAULT PERSPECTIVE
        float d_Distance = 5;
        public override void DefaultProcess(PerspectiveViewerWindow window)
        {
            int scroll = Program.Game.PreviousMouse.ScrollWheelValue - Program.Game.CurrentMouse.ScrollWheelValue;
            if (scroll > 0) scroll = 1;
            if (scroll < 0) scroll = -1;

            d_Distance += (float)(scroll / Program.Game.FrameRate) * 10;
            window.Camera.Position = window.Camera.TargetAngle * -d_Distance;

            if (Program.Game.CurrentKeyboard.IsKeyDown(Keys.R) && Program.Game.PreviousKeyboard.IsKeyUp(Keys.R))
            {
                renderPlanes = !renderPlanes;
            }
        }
        public override void DefaultRender(PerspectiveViewerWindow window)
        {
            _graphics.Clear(Color.CornflowerBlue);
            _graphics.DepthStencilState = _dss;

            window.Camera.SetupDraw();

            if (CurrentBrush != null)
            {
                Vector3 temp = CurrentBrush.physical.position;
                CurrentBrush.physical.position = Vector3.Zero;

                //TODO: Optional mode to render only selected polygon
                CurrentBrush.physical.Draw(window.Camera);

                CurrentBrush.physical.position = temp;
            }

            if (renderPlanes)
            {
                for (int i = 1; i < 4; i++)
                {
                    xyzPlanes[i].position = windows[i].Camera.Position;
                    xyzPlanes[i].Draw(window.Camera);
                }
            }

            Program.SpritesBeginDefault(_sprites);
            _sprites.DrawString(font, names[0], Vector2.Zero, colors[0]);

            _sprites.End();
        }

        //Orthos
        public override void OrthoProcess(PerspectiveViewerWindow window, WindowContext context)
        {
            base.OrthoProcess(window, context);
        }
        public override void OrthoRender(PerspectiveViewerWindow window, WindowContext context)
        {
            _graphics.Clear(Color.Black);

            if (CurrentBrush != null)
            {
                window.Camera.SetupDraw();

                Vector3 temp = CurrentBrush.physical.position;
                CurrentBrush.physical.position = Vector3.Zero;

                if (brushContentsList.curIndex == -1) //whole shape
                {
                    CurrentBrush.physical.Draw(window.Camera);

                    CurrentBrush.physical.position = temp;
                }

                else //draw single poly + dots to represent its verts
                {
                    CurrentBrush.physical.Draw(window.Camera, brushContentsList.curIndex);
                }
            }

            base.OrthoRender(window, context);
        }
    }
}
