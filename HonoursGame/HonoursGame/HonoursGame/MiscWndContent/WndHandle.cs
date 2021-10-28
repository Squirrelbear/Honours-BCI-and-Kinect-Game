using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class WndHandle
    {
        public enum WndType { MainMenu, TilePuzzle, RiverPuzzle, StreetPuzzle, InfoWnd, Quit, ConfirmDialog, VictoryDialog, FailureDialog, Calibration };
        protected WndType wndType;
        protected Game1 appRef;
        protected InputManager inputManager;
        protected int wndWidth, wndHeight;

        public WndHandle(WndType wndType, int wndWidth, int wndHeight, Game1 appRef)
        {
            this.wndType = wndType;
            this.appRef = appRef;
            this.wndWidth = wndWidth;
            this.wndHeight = wndHeight;
            this.inputManager = appRef.getInputManager();
        }

        public WndType getWndType()
        {
            return wndType;
        }

        public Game1 getAppRef()
        {
            return appRef;
        }

        public virtual void update(GameTime gameTime)
        {
            // do nothing
        }

        public virtual void draw(SpriteBatch spriteBatch)
        {
            // do nothing
        }

        public virtual void handleSpeechRecognised(string s)
        {
            // do nothing
        }

        public virtual void handleSpeechNotRecognised()
        {
            // do nothing
        }
    }
}
