using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class PlayerObj : AnimatedObj
    {
        private static int WIDTH = 256;//128;
        private static int HEIGHT = 180;//128;
        private int lane;
        private int rowStartX;
        private int colWidth;
        private bool stopped;

        public PlayerObj(int rowStartX, int colWidth, Game1 appRef, Point startPoint)
            : base(appRef.Content.Load<Texture2D>("StreetPuzzle//chargreen"),
                   WIDTH, HEIGHT,
                   new Rectangle(startPoint.X, startPoint.Y, 40, 40))
        {
            this.rowStartX = rowStartX;
            this.colWidth = colWidth;
            setLane(1);
            start();
        }

        public override void update(GameTime gameTime)
        {
            base.update(gameTime);
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            base.draw(spriteBatch);
        }

        public void setLane(int lane)
        {
            this.lane = lane;
            x = rowStartX + colWidth * lane + colWidth / 2 - dest.Width / 2;
            dest.X = (int)x;
        }

        public int getLane()
        {
            return lane;
        }

        public void stop()
        {
            stopped = true;
            beginAnimation(0, 0);
        }

        public void start()
        {
            stopped = false;
            beginAnimation(0, 6);
        }

        public bool getStopped()
        {
            return stopped;
        }
    }
}
