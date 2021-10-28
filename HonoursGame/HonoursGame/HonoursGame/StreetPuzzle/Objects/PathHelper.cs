using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class PathHelper : ObstacleObj
    {
        public enum Direction { Up, Left, Right };

        private int pathID;
        private Direction nextDirection;
        private bool showDebug;
        private bool stopHandled;

        public PathHelper(ObstacleObj.ObstacleType type, int pathID, Direction nextDirection, Rectangle dest, Texture2D overlaySprite)
            : base(type, dest, overlaySprite, overlaySprite)
        {
            this.pathID = pathID;
            this.nextDirection = nextDirection;

            if (type == ObstacleType.LevelEnd)
            {
                enablePlayerCollisions = true;
            }
            else
            {
                enablePlayerCollisions = false;
            }

            showDebug = false;
            stopHandled = false;
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            if (!showDebug) return;

            if (collided)
            {
                spriteBatch.Draw(spriteMain, dest, source, Color.Green, 0, origin, SpriteEffects.None, 0);
            }
            else
            {
                spriteBatch.Draw(spriteMain, dest, source, Color.Red, 0, origin, SpriteEffects.None, 0);
            }
        }

        public Direction getDirection()
        {
            return nextDirection;
        }

        public int getPathID()
        {
            return pathID;
        }

        public bool canTriggerMove(int objY, int objHeight)
        {
            return (dest.Y + dest.Height > objY + objHeight);
        }

        public bool isStopHandled()
        {
            return stopHandled;
        }

        public void handleStop()
        {
            stopHandled = true;
        }
    }
}
