using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class AnimatedObj
    {
        protected Texture2D spriteSheet;

        protected float speedMultiplierX;
        protected float speedMultiplierY;
        protected float x, y;
        protected float rotation;
        protected Rectangle dest, source;

        protected int spriteWidth, spriteHeight;
        protected int spriteSheetWidth, spriteSheetHeight;
        protected int framesPerRow;
        protected int animStartID, animEndID;
        protected int animCurID;
        protected float opacity;
        protected Color color;
        protected Vector2 origin;
        protected int playforframes;

        private float nextFrameTimer;
        private float frameTime = 250;

        public AnimatedObj(Texture2D spriteSheet, int spriteWidth, int spriteHeight, Rectangle dest)
        {
            this.spriteSheet = spriteSheet;
            this.spriteWidth = spriteWidth;
            this.spriteHeight = spriteHeight;
            this.dest = dest;

            speedMultiplierX = 0;
            speedMultiplierY = 0;
            x = dest.X;
            y = dest.Y;
            rotation = 0;
            spriteSheetWidth = spriteSheet.Width;
            spriteSheetHeight = spriteSheet.Height;
            framesPerRow = spriteSheetWidth / spriteWidth;
            animStartID = animCurID = animEndID = 0;
            source = new Rectangle(0, 0, spriteWidth, spriteHeight);

            opacity = 1.0f;
            color = Color.White;
            origin = new Vector2(0, 0);//spriteWidth / 2, spriteHeight / 2);

            nextFrameTimer = 0;
            playforframes = 0;
        }

        public virtual void update(GameTime gameTime)
        {
            if (animStartID != animEndID)
            {
                nextFrameTimer -= gameTime.ElapsedGameTime.Milliseconds;

                if (nextFrameTimer <= 0)
                {
                    if (animCurID == animEndID)
                    {
                        animCurID = animStartID;
                    }
                    else
                    {
                        animCurID++;
                    }
                    setFrame(animCurID);

                    if (playforframes > 0)
                    {
                        playforframes--;
                    }
                    else if (playforframes == 0)
                    {
                        beginAnimation(0, 0);
                    }
                    nextFrameTimer = frameTime;
                }
            }
        }

        public virtual void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(spriteSheet, dest, source, color * opacity, 
                              rotation, origin, SpriteEffects.None, 0);
        }

        public void beginAnimation(int startFrameID, int endFrameID)
        {
            animStartID = startFrameID;
            animEndID = endFrameID;
            animCurID = startFrameID;
            nextFrameTimer = frameTime;
            setFrame(animStartID);
            playforframes = -1;
        }

        public void beginAnimation(int startFrameID, int endFrameID, int playforframes)
        {
            animStartID = startFrameID;
            animEndID = endFrameID;
            animCurID = startFrameID;
            nextFrameTimer = frameTime;
            setFrame(animStartID);
            this.playforframes = playforframes;
        }

        public void setFrame(int frameID)
        {
            animCurID = frameID;
            int row = frameID / framesPerRow;
            int col = frameID % framesPerRow;
            source = new Rectangle(col*spriteWidth, row*spriteHeight, spriteWidth, spriteHeight);
        }

        public void setColor(Color color)
        {
            this.color = color;
        }

        public void setOpacity(float opacity)
        {
            this.opacity = opacity;
        }

        public void setRotation(float rotation)
        {
            this.rotation = rotation;
        }

        public Rectangle getRect()
        {
            return dest;
        }

        public void setLocation(float x, float y)
        {
            this.x = x;
            this.y = y;
            dest.X = (int)this.x;
            dest.Y = (int)this.y;
        }

        public void moveBy(float x, float y)
        {
            this.x += x;
            this.y += y;
            dest.X = (int)this.x;
            dest.Y = (int)this.y;
        }

        public float getX()
        {
            return x;
        }

        public float getY()
        {
            return y;
        }

        public void setSpriteSheet(Texture2D spriteSheet)
        {
            this.spriteSheet = spriteSheet;
        }

        public void setDefaultOrigin()
        {
            origin = new Vector2(spriteWidth / 2, spriteSheetHeight / 2);
        }

        public bool isAnimating()
        {
            return animStartID != animEndID;
        }
    }
}
