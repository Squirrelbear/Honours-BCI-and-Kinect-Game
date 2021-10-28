using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class CalibratorTile
    {
        private Rectangle dest, emptyDest, emptyDest2;
        private float y2;

        // Graphics variables:
        protected Texture2D spriteBase;
        protected Texture2D spriteEmptyCell;

        private int minY, maxY;

        public CalibratorTile(int wndWidth, int wndHeight, Game1 appRef)
        {
            // Load textures
            spriteBase = appRef.Content.Load<Texture2D>("TilePuzzle\\t1");
            spriteEmptyCell = appRef.Content.Load<Texture2D>("TilePuzzle\\emptytile");

            int width = 200;
            int height = 200;

            int x = (wndWidth - width) / 2;
            int y1 = 50;
            y2 = (wndHeight - height) / 2;
            int y3 = wndHeight - height - y1;

            dest = new Rectangle(x, (int)y2, width, height);
            emptyDest = new Rectangle(x, y1, width, height);
            emptyDest2 = new Rectangle(x, y3, width, height);

            minY = emptyDest.Y;
            maxY = emptyDest2.Y;
        }

        public void update(GameTime gameTime)
        {
            
        }

        public void draw(SpriteBatch spriteBatch)
        {
            // render the empty cell in the current cell
            spriteBatch.Draw(spriteEmptyCell, emptyDest, Color.White);
            spriteBatch.Draw(spriteEmptyCell, emptyDest2, Color.White);
            spriteBatch.Draw(spriteBase, dest, Color.White);
        }

        public void move(float yMod)
        {
            y2 += yMod;
            if (y2 > maxY) y2 = maxY;
            if (y2 < minY) y2 = minY;

            dest.Y = (int)y2;
        }

        public void setRelativeLocation(float percent)
        {
            dest.Y = (int)(minY + (maxY - minY) * percent);
        }
    }
}
