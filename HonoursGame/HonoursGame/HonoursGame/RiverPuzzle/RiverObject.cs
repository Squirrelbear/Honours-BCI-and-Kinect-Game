using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class RiverObject : AnimatedObj
    {
        public enum ObjectType { Junk, Treasure };

        private ObjectType objectType;
        private bool selected;
        private Vector2 offset;

        private AnimatedObj waterOverlay;

        // the numbers at the time of grabbing the object
        private float visibility, speed;
        private float curX;

        private bool failedObj;

        public RiverObject(ObjectType objectType, Vector2 offset, Texture2D spriteSheet, Texture2D waterOverTexture, Rectangle dest)
            : base(spriteSheet, spriteSheet.Width, spriteSheet.Height, dest)
        {
            this.objectType = objectType;
            selected = false;
            failedObj = false;

            // force to use origin for rotation
            setDefaultOrigin();
            //setRotation

            if (objectType == ObjectType.Treasure)
            {
                setColor(Color.Yellow);
            }

            this.curX = dest.X;
            this.offset = offset;

            Rectangle destWater = new Rectangle(dest.X, dest.Bottom - 40, waterOverTexture.Width, waterOverTexture.Height);
            waterOverlay = new AnimatedObj(waterOverTexture, waterOverTexture.Width, waterOverTexture.Height, destWater);
        }

        public void update(GameTime gameTime, float visibility, float speed)
        {
            base.update(gameTime);

            if (!selected)
            {
                this.visibility = visibility;
                this.speed = speed;

                curX -= gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 100 * (speed + 0.1f);
                dest.X = (int)curX;
                dest.Y = (int)(Math.Sin(curX * gameTime.ElapsedGameTime.Milliseconds / 1000.0 + offset.X) * 20 +offset.Y);
            }
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            base.draw(spriteBatch);

            waterOverlay.draw(spriteBatch);
        }

        public bool isPointInThis(Point p)
        {
            return dest.Contains(p);
        }

        public Rectangle getBoundBox()
        {
            Rectangle result = new Rectangle(dest.X - 50, dest.Y - 50, dest.Width, dest.Height);
            return result;
        }

        public void setSelected(bool selected)
        {
            this.selected = selected;
        }

        public void select()
        {
            selected = true;
        }

        public bool isSelected()
        {
            return selected;
        }

        public ObjectType getObjectType()
        {
            return objectType;
        }

        public bool isFailed()
        {
            return failedObj;
        }

        public void failObject()
        {
            failedObj = true;
        }
    }
}
