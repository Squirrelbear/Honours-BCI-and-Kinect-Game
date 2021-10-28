using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class PeopleObstacle : ObstacleObj
    {
        Texture2D charSprite, speechNormalSprite, speechCollidedSprite;
        AnimatedObj person1, speech1;//person2, speech1, speech2;
        Texture2D spriteDebug;
        Random gen;
        float speechCooldown;

        public PeopleObstacle(Rectangle dest, Texture2D charSprite, Texture2D speechNormalSprite, Texture2D speechCollidedSprite, Texture2D spriteDebug, Random gen)
            : base(ObstacleType.ByStanders, dest, charSprite, charSprite)
        {
            this.charSprite = charSprite;
            this.speechNormalSprite = speechNormalSprite;
            this.speechCollidedSprite = speechCollidedSprite;
            this.spriteDebug = spriteDebug;

            this.gen = gen;

            Rectangle person1Dest = new Rectangle(dest.X-256, dest.Y-256, 40, 40);
            person1 = new AnimatedObj(charSprite, 256, 180, person1Dest);
            person1.setRotation(MathHelper.PiOver4 * gen.Next(0,5));
            person1.setDefaultOrigin();

            /*Rectangle person2Dest = new Rectangle(dest.Right, dest.Y + 30, 40, 40);
            person2 = new AnimatedObj(charSprite, 256, 180, person2Dest);
            person2.setRotation(MathHelper.PiOver4 * 3);*/

            speech1 = new AnimatedObj(speechNormalSprite, 256, 180, 
                                        new Rectangle(person1Dest.X - 35, person1Dest.Y, 25, 25));
            /*speech2 = new AnimatedObj(speechNormalSprite, 256, 180, 
                                        new Rectangle(person2Dest.X + 15, person2Dest.Y - 40, 40, 40));*/
            speech1.beginAnimation(0, 4, gen.Next(5,15));
            speechCooldown = -1;
        }

        public override void update(GameTime gameTime, float displaceY)
        {
            base.update(gameTime, displaceY);

            person1.update(gameTime);
            person1.setLocation(dest.X + 25, dest.Y + 40);
            //person2.update(gameTime);
            //person2.setLocation(dest.Right, dest.Y + 10);
            speech1.update(gameTime);
            speech1.setLocation(person1.getRect().X + 15, person1.getRect().Y - 40);
            //speech2.update(gameTime);
            //speech2.setLocation(person2.getRect().X + 15, person2.getRect().Y - 40);

            if (speechCooldown != -1 && !speech1.isAnimating())
            {
                speechCooldown -= gameTime.ElapsedGameTime.Milliseconds;

                if (speechCooldown <= 0)
                {
                    speechCooldown = -1;
                    speech1.beginAnimation(0, 4, gen.Next(5, 15));
                }
            }
            else if(speechCooldown == -1 && !speech1.isAnimating())
            {
                speechCooldown = gen.Next(1000, 1500);
            }
        } 

        public override void draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(spriteDebug, dest, new Rectangle(0,0,spriteDebug.Width,spriteDebug.Height), Color.Green, 0, origin, SpriteEffects.None, 0);
            person1.draw(spriteBatch);
            //person2.draw(spriteBatch);
            if(speech1.isAnimating())
                speech1.draw(spriteBatch);
            //speech2.draw(spriteBatch);
        }

        public override void triggerCollided()
        {
            if (collided) return;
            
            base.triggerCollided();

            speech1.setSpriteSheet(speechCollidedSprite);
            //speech2.setSpriteSheet(speechCollidedSprite);
            speech1.beginAnimation(0, 4, gen.Next(10,15));
            //speech2.beginAnimation(0, 4);
        }
    }
}
