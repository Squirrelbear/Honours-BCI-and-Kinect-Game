using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class ObstacleObj
    {
        public enum ObstacleType { MarketStall, ByStanders, Crate, River, RiverOverpass, Path, PathStop, LevelEnd };

        protected Texture2D spriteMain;
        protected Texture2D spriteInteracted;
        protected Rectangle dest, source;
        protected ObstacleType obsType;
        protected bool collided;
        protected float positionY;
        protected Vector2 origin;
        protected bool enablePlayerCollisions;

        public ObstacleObj(ObstacleType obsType, Rectangle dest, Texture2D spriteMain, Texture2D spriteInteracted)
        {
            this.dest = dest;
            positionY = dest.Y;
            this.spriteMain = spriteMain;
            this.spriteInteracted = spriteInteracted;
            collided = false;
            this.obsType = obsType;
            source = new Rectangle(0, 0, spriteMain.Width, spriteMain.Height);
            origin = new Vector2(0, 0);//dest.Width / 2, dest.Y / 2);
            if (obsType == ObstacleType.RiverOverpass)
            {
                enablePlayerCollisions = false;
            }
            else
            {
                enablePlayerCollisions = true;
            }
        }

        public virtual void update(GameTime gameTime, float displaceY)
        {
            positionY += displaceY;
            dest.Y = (int)positionY;
        }

        public virtual void draw(SpriteBatch spriteBatch)
        {
            if (collided)
            {
                spriteBatch.Draw(spriteInteracted, dest, source, Color.White, 0, origin, SpriteEffects.None, 0);
            }
            else
            {
                spriteBatch.Draw(spriteMain, dest, source, Color.White, 0, origin, SpriteEffects.None, 0);
            }
        }

        public bool collidingWith(Rectangle rect)
        {
            return dest.Intersects(rect);
        }

        public bool isCollided()
        {
            return collided;
        }

        public virtual void triggerCollided()
        {
            collided = true;
        }

        public bool hasExitedScreen(int wndHeight)
        {
            return (dest.Y > wndHeight);
        }

        public float getPositionY()
        {
            return positionY;
        }

        public bool canCollidePlayer()
        {
            return enablePlayerCollisions;
        }

        public ObstacleType getType()
        {
            return obsType;
        }
    }
}
