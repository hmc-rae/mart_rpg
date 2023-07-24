using core.src;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace core
{
    public class martgame : Game
    {
        private GraphicsDeviceManager _graphics;
        internal SpriteBatch _spriteBatch;
        private DepthStencilState _def;

        public Console CommandConsole;
        public double FrameRate;
        public double FrameTime;

        private GameScene curscene;
        private GameScene nexscene;

        private KeyboardState _prev, _cur;
        private MouseState _pms, _cms;
        public KeyboardState PrevKey => _prev;
        public KeyboardState PreviousKeyboard => _prev;
        public KeyboardState CurKey => _cur;
        public KeyboardState CurrentKeyboard => _cur;

        public MouseState PreviousMouse => _pms;
        public MouseState CurrentMouse => _cms;

        public martgame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _def = DepthStencilState.Default;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _prev = new KeyboardState();
            _cur = new KeyboardState();

            _pms = new MouseState();
            _cms = new MouseState();

            CommandConsole = new Console();

            curscene = new GameScene();

            _def = DepthStencilState.DepthRead;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();

            base.Initialize();

            CommandConsole.Initialize(this);
            CommandConsole.RegisterCommand("scene", loadscene, "Changes the current game scene.");
            curscene.Init(this);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            curscene.Load(this.Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (nexscene != null)
            {
                curscene.OnDestroy();
                curscene = nexscene;
                nexscene = null;
            }

            _prev = _cur;
            _cur = Keyboard.GetState();

            _pms = _cms;
            _cms = Mouse.GetState();
            FrameTime = gameTime.ElapsedGameTime.TotalSeconds;
            FrameRate = (1 / gameTime.ElapsedGameTime.TotalSeconds);

            
            CommandConsole.Update(gameTime);
#if DEBUG
            if (KeyFall(Keys.OemTilde))
                CommandConsole.Toggle();

            if (!CommandConsole.IsVisible)
            {
#endif
                curscene.OnFrame();
#if DEBUG
            }
#endif

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Add all predraw code here (to targets
            CommandConsole.PreDraw();

            curscene.PreRender();

            // Add all normal draw code
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = _def;

            curscene.OnRender();

            CommandConsole.Draw();

            base.Draw(gameTime);
        }

        public bool KeyFall(Keys key)
        {
            return _cur.IsKeyDown(key) && _prev.IsKeyUp(key);
        }

        private int loadscene(string[] args)
        {
            if (args.Length == 0) return -1;
            switch (args[0].ToLower())
            {
                case "default":
                    nexscene = new GameScene();
                    break;
                case "mapedit":
                    nexscene = new MapEdit();
                    break;
            }
            if (nexscene == null) return -2;
            nexscene.Init(this);
            nexscene.Load(Content);
            return 0;
        }
    }
}