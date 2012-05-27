using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace second3d
{
    public class InputHandler 
        : Microsoft.Xna.Framework.GameComponent, IInputHandler
    {
        private KeyboardHandler keyboard;

#if !XBOX360
        private MouseHandler mouse;
#endif

        public InputHandler(Game game)
            : base(game)
        {
            game.Services.AddService(typeof(IInputHandler), this);
            keyboard = new KeyboardHandler();

#if !XBOX360
            mouse = new MouseHandler();
            Game.IsMouseVisible = true;
#endif
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            keyboard.Update();
            mouse.Update();

            base.Update(gameTime);
        }

        #region IInputHandler Members

        public KeyboardHandler KeyboardHandler
        {
            get { return (keyboard); }
        }

        public MouseHandler MouseHandler
        {
            get { return (mouse); }
        }

        #endregion
    }
}
