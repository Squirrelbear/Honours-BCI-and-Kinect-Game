using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class SpriteButton
    {
        private Texture2D spriteOver;
        private Texture2D spriteOut;

        private Rectangle dest;
        private bool isOver;
        private int actionID;

        public SpriteButton(Rectangle dest, Texture2D spriteOver, Texture2D spriteOut, int actionID)
        {
            this.dest = dest;
            this.spriteOver = spriteOver;
            this.spriteOut = spriteOut;
            this.actionID = actionID;
            isOver = false;
        }

        public void update(GameTime gameTime)
        {
            // do nothing
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (isOver)
            {
                spriteBatch.Draw(spriteOver, dest, Color.White);
            }
            else
            {
                spriteBatch.Draw(spriteOut, dest, Color.White);
            }
        }

        public int getActionID()
        {
            return actionID;
        }

        public void setOver(bool isOver)
        {
            this.isOver = isOver;
        }

        public bool getOver()
        {
            return isOver;
        }

        public bool isPointInButton(Point p)
        {
            return dest.Contains(p);
        }

        public Rectangle getRect()
        {
            return dest;
        }
    }
}
