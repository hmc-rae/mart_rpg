using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core
{
    public class GameScene
    {
        public imsimgame _game;

        public void Init(imsimgame game)
        {
            _game = game;

            OnCreate();
        }

        public virtual void OnCreate() { }
        public virtual void Load(ContentManager content) { }
        public virtual void OnFrame() { }
        public virtual void PreRender() { }
        public virtual void OnRender() { }
        public virtual void OnDestroy() { }
    }
}
