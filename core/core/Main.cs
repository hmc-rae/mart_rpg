using core.src;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using prog;
using System;

namespace core
{
    public class martgame : Game
    {
        private GraphicsDeviceManager _graphics;
        internal SpriteBatch _spriteBatch;
        private DepthStencilState _def;
        private RenderTarget2D facade;
        private Rectangle cull;
        private Vector2 dim;
        private Vector2 scal;

        public Console CommandConsole;
        public double FrameRate;
        public double FrameTime;

        private GameScene curscene;
        private GameScene nexscene;

        private int ss;
        public martgame()
        {
            scal = new Vector2(Program.ExternalScreen.X / Program.InternalScreen.X, Program.ExternalScreen.Y / Program.InternalScreen.Y);

            _graphics = new GraphicsDeviceManager(this);
            _def = DepthStencilState.Default;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            CommandConsole = new Console();

            curscene = new GameScene();

            _def = DepthStencilState.DepthRead;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            _graphics.PreferredBackBufferWidth = (int)Program.ExternalScreen.X;
            _graphics.PreferredBackBufferHeight = (int)Program.ExternalScreen.Y;
            _graphics.ApplyChanges();

            base.Initialize();

            facade = new RenderTarget2D(GraphicsDevice, (int)Program.InternalScreen.X, (int)Program.InternalScreen.Y, true, SurfaceFormat.Alpha8, DepthFormat.Depth24Stencil8);
            dim = Vector2.Zero;
            cull = new Rectangle(0, 0, (int)Program.InternalScreen.X, (int)Program.InternalScreen.Y);

            Input.Init();


            CommandConsole.Initialize(this);
            CommandConsole.RegisterCommand("scene", loadscene, "Changes the current game scene.");
            curscene.Init(this);

            Program.InitDrawLine(_graphics.GraphicsDevice);

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

            Input.Poll(Keyboard.GetState(), Mouse.GetState());

            FrameTime = gameTime.ElapsedGameTime.TotalSeconds;
            FrameRate = (1 / gameTime.ElapsedGameTime.TotalSeconds);

            
            CommandConsole.Update(gameTime);
#if DEBUG
            if (Input.IsKeyPressed(Keys.OemTilde))
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
            GraphicsDevice.SetRenderTarget(facade);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = _def;

            curscene.OnRender();

            CommandConsole.Draw();

            GraphicsDevice.SetRenderTarget(null);

            prog.Program.SpritesBeginDefault(_spriteBatch);

            _spriteBatch.Draw(facade, dim, cull, Color.White, 0, dim, scal, SpriteEffects.None, 0f);

            _spriteBatch.End();

            base.Draw(gameTime);
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