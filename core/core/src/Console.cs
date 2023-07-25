using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using System;

using System.Threading;

using Microsoft.Xna.Framework.Input;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using prog;

namespace core
{
    public class Console
    {
        private RenderTarget2D _render;
        private Rectangle _cull;
        private GraphicsDevice _graphics;
        private DepthStencilState _def;
        private SpriteBatch _sprites;
        private Texture2D _rect;
        private Vector2 _anchorpos;
        private Color backdrop;

        private SpriteFont font;

        public string curString;
        public string[] listStrings;
        public int listStartIndex;
        public int listDepth;

        public bool IsVisible;

        private martgame _game;
        private const int listcap = 16;
        public void Initialize(martgame game)
        {
            _game = game;
            _graphics = _game.GraphicsDevice;
            _def = new DepthStencilState() { DepthBufferEnable = true };
            _sprites = _game._spriteBatch;
            _render = new RenderTarget2D(_graphics, (int)Program.InternalScreen.X, (int)Program.InternalScreen.Y/2, true, _graphics.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            _rect = new Texture2D(_graphics, (int)Program.InternalScreen.X, 50);

            _cull = new Rectangle(0, 0, (int)Program.InternalScreen.X, (int)Program.InternalScreen.Y / 2);

            Color[] data = new Color[50 * (int)Program.InternalScreen.X];
            for (int i = 0; i < data.Length; i++) data[i] = Color.Gray;

            _rect.SetData(data);

            font = _game.Content.Load<SpriteFont>("Fonts/Arial25");

            curString = "";
            listStrings = new string[listcap];

            listStartIndex = listcap;
            listDepth = 0;

            backdrop = new Color(0, 0, 0, 100);

            _hideY = -_render.Height;

            commands = new Dictionary<string, Func<string[], int>>();
            commandDesc = new Dictionary<string, string>();
            commandListRaw = new List<string>();
            RegisterCommand("help", (string[] args) =>
            {
                if (args.Length == 0)
                {
                    PushString("help (commandname)");
                    return 0;
                }

                string tag = args[0].ToLower();
                if (!commands.ContainsKey(tag))
                {
                    PushString("help (commandname)");
                    return 0;
                }

                PushString($"{commandDesc[tag]}");
                return 0;
            }, "Provides information on a command.");

            RegisterCommand("clear", (string[] arg) =>
            {
                listStartIndex = listcap;
                listDepth = 0;
                return 0;
            }, "Clears the console window.");

            RegisterCommand("quit", (string[] arg) =>
            {
                Environment.Exit(1);
                return 0;
            }, "Closes the game.");

            int pagesize = 10;
            RegisterCommand("list", (string[] args) =>
            {
                int page = 1;
                if (args.Length > 0)
                {
                    int.TryParse(args[0], out page);
                }

                page = page - 1;
                if (page < 0) return 0;
                PushString($"Commands (page {page+1} / {(commandcount / pagesize) + (commandcount % pagesize > 0 ? 1 : 0)})");
                for (int i = page * pagesize, k = 0; i < commandcount && k < pagesize; i++, k++)
                {
                    PushString($"    {commandListRaw[i]}");
                }
                return 0;

            }, "Prints a list of all the commands.");


        }


        private int _hideY = 0;
        private int rate = 60;
        public void Toggle()
        {
            IsVisible = !IsVisible;
            curString = "";
        }
        public void PushString(string line)
        {
            if (listDepth < listcap)
            {
                listStartIndex--;
                listDepth++;
                listStrings[listStartIndex] = line;
            }
            else
            {
                listStartIndex -= 1;
                if (listStartIndex < 0) listStartIndex = listcap + listStartIndex;
                listStrings[listStartIndex] = line;
            }
        }
        /// <summary>
        /// Returns true if the command was successfully added.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public bool RegisterCommand(string tag, Func<string[], int> func, string desc = "An externally added command.")
        {
            tag = tag.ToLower();
            if (commands.ContainsKey(tag)) return false;

            commands.Add(tag, func);
            commandDesc.Add(tag, desc);
            commandListRaw.Add(tag);
            commandcount++;
            return true;
        }
        /// <summary>
        /// Returns true if the command was successfully removed.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool DeregisterCommand(string tag)
        {
            tag = tag.ToLower();
            if (!commands.ContainsKey(tag)) return false;

            commands.Remove(tag);
            commandDesc.Remove(tag);
            commandListRaw.Remove(tag);
            commandcount--;
            return true;
        }
        public int Invoke(string line)
        {
            string[] args = line.Split(" ");
            args[0] = args[0].ToLower();
            if (!commands.ContainsKey(args[0])) return -1;

            string[] temp = new string[args.Length - 1];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = args[i + 1];
            }

            return commands[args[0]](temp);
        }

        private Dictionary<string, Func<string[], int>> commands;
        private Dictionary<string, string> commandDesc;
        private List<string> commandListRaw;
        private int commandcount = 0;
        private double backcooldown = 0;
        public void Update(GameTime gameTime)
        {
            if (!IsVisible) return;

            bool shift = Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift);
            for (int i = 0; i < 256; i++)
            {
                if (!Enum.IsDefined(typeof(Keys), i)) continue;

                if (Input.IsKeyPressed((Keys)i))
                {
                    char ret = Input.GetChar((Keys)i, shift);
                    if (ret != 0)
                        curString += ret;
                }
            }

            if (Input.IsKeyPressed(Keys.Enter))
            {
                //Push curstring and call thing to it
                PushString(curString);
                Invoke(curString);
                curString = "";

            }

            if (Input.IsKeyDown(Keys.Back))
            {
                if (curString.Length > 0 && backcooldown <= 0)
                {
                    curString = curString.Remove(curString.Length - 1, 1);
                    backcooldown = 0.1;
                }
            }
            if (backcooldown > 0)
            {
                backcooldown -= _game.FrameTime;
            }
        }
        public void PreDraw()
        {
            if (_hideY <= -_render.Height && !IsVisible) return;
            if (!IsVisible) _hideY -= rate;
            else _hideY += rate;
            if (_hideY >= 0) _hideY = 0;

            _graphics.SetRenderTarget(_render);

            _graphics.DepthStencilState = _def;

            _graphics.Clear(backdrop);

            _sprites.Begin();

            _anchorpos.X = _render.Width - 100;
            _anchorpos.Y = 0;

            _sprites.DrawString(font, _game.FrameRate.ToString(), _anchorpos, Color.Black);

            _anchorpos.X = 0;
            _anchorpos.Y = _render.Height - 50;

            _sprites.Draw(_rect, _anchorpos, Color.Gray);

            _anchorpos.X = 25;

            //Draw first userinp string

            _sprites.DrawString(font, "USER>> " + curString, _anchorpos, Color.Black);

            _anchorpos.Y -= 10;
            //Draw N entries from the cyclic array of strings
            for (int i = listStartIndex, k = 0; k < listDepth; k++, i = (i + 1) % listcap)
            {
                _anchorpos.Y -= 30;
                _sprites.DrawString(font, listStrings[i], _anchorpos, Color.Black);
            }

            _sprites.End();
        }
        public void Draw()
        {
            _sprites.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.Default,
                RasterizerState.CullNone);

            _anchorpos.X = 0;
            _anchorpos.Y = _hideY;

            _sprites.Draw(_render, _anchorpos, _cull, Color.White);

            _sprites.End();
        }
    }

}
