using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HonoursGame
{
    public class Cursor
    {
        public enum SelectionState { None, InProgress, Complete };

        protected Point point;
        protected Texture2D handSprite;
        protected Rectangle dest;

        protected Rectangle destProgress;
        protected Rectangle srcProgress;
        protected Texture2D progressSprite;

        private int selectionCountdown;
        private int totalCountdown;
        private int curFrame;
        private Rectangle selectionBBox;
        private SelectionState selectionState;

        public Cursor(Texture2D handSprite, Texture2D progressSprite)
        {
            point = new Point(100, 100);
            this.handSprite = handSprite;
            dest = new Rectangle(point.X, point.Y, handSprite.Width, handSprite.Height);

            this.progressSprite = progressSprite;
            srcProgress = new Rectangle(0, 0, 62, 62);
            selectionState = SelectionState.None;
        }

        public void update(GameTime gameTime)
        {
            /*MouseState mouseState = game.getMouseState();
            point.X = mouseState.X;
            point.Y = mouseState.Y;*/

            dest = new Rectangle(point.X, point.Y, handSprite.Width, handSprite.Height);

            if (selectionState != SelectionState.None)
            {
                destProgress = new Rectangle(point.X-20, point.Y-15, handSprite.Width, handSprite.Height);
                if (selectionState == SelectionState.Complete) 
                    return;

                selectionCountdown -= gameTime.ElapsedGameTime.Milliseconds;
                if (selectionCountdown <= 0)
                {
                    selectionState = SelectionState.Complete;
                    // set frame to last frame
                    curFrame = 8;
                    srcProgress.X = curFrame * 62;
                }
                else if (!selectionBBox.Contains(point))
                {
                    clearSelectionProgress();
                }
                else
                {
                    // update frame id
                    curFrame = (int)((1 - selectionCountdown * 1.0 / totalCountdown) * 8);
                    srcProgress.X = curFrame * 62;
                }
            }
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (selectionState != SelectionState.None)
            {
                spriteBatch.Draw(progressSprite, destProgress, srcProgress, Color.White, 0, new Vector2(0,0), SpriteEffects.None, 0);
            }
            
            spriteBatch.Draw(handSprite, dest, Color.White);
        }

        public Point getPoint()
        {
            return point;
        }

        public void setPoint(Vector2 point)
        {
            this.point.X = (int)point.X;
            this.point.Y = (int)point.Y;
        }

        public void beginSelection(int time, Rectangle selectionBBox)
        {
            destProgress = new Rectangle(point.X - 20, point.Y - 15, handSprite.Width, handSprite.Height);
            this.totalCountdown = this.selectionCountdown = time;
            this.selectionBBox = selectionBBox;
            curFrame = 0;
            selectionState = SelectionState.InProgress;
        }

        public void updateSelectionBox(Rectangle selectionBBox)
        {
            this.selectionBBox = selectionBBox;
        }

        public bool isSelectionComplete()
        {
            return selectionState == SelectionState.Complete;
        }

        public void clearSelectionProgress()
        {
            selectionState = SelectionState.None;
        }

        public bool isSelecting()
        {
            return selectionState != SelectionState.None;
        }
    }
}
