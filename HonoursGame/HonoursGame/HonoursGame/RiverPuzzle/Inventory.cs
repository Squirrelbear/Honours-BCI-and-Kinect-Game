using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class Inventory
    {
        private Texture2D junkTexture, treasureTexture;//, timeTexture;
        private Rectangle junkDest, treasureDest;
        
        private Rectangle boundingBox;
        private int junkCount;
        private int treasureCount;
        private int score;

        private float timeSpeed;
        private float visibility;
        private float levelTime;
        private float realTime;
        private float modifiedTime;

        private SpriteFont font;

        private RiverPuzzle puzzle;

        public Inventory(float levelTime, Rectangle boundingBox, Texture2D junkTexture, Texture2D treasureTexture, SpriteFont font, RiverPuzzle puzzle)
        {
            this.boundingBox = boundingBox;
            this.junkTexture = junkTexture;
            this.treasureTexture = treasureTexture;
            this.font = font;
            this.puzzle = puzzle;

            junkDest = new Rectangle(boundingBox.Right - 440, boundingBox.Top, 180, 180);
            treasureDest = new Rectangle(boundingBox.Right - 210, boundingBox.Top, 180, 180);

            score = 0;
            junkCount = 0;
            treasureCount = 0;
            timeSpeed = 1;
            visibility = 0;
            this.levelTime = levelTime;
            realTime = 0;
            modifiedTime = 0;
        }

        public void update(GameTime gameTime, float timeSpeed, float visibility)
        {
            this.timeSpeed = timeSpeed;
            this.visibility = visibility;

            realTime += gameTime.ElapsedGameTime.Milliseconds;
            modifiedTime += gameTime.ElapsedGameTime.Milliseconds * (timeSpeed);

            if (modifiedTime > levelTime)
            {
                puzzle.endLevel(score, junkCount, treasureCount);
            }
        }

        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(treasureTexture, treasureDest, Color.White);
            spriteBatch.Draw(junkTexture, junkDest, Color.White);

            spriteBatch.Draw(junkTexture, junkDest, Color.White);
            spriteBatch.Draw(treasureTexture, treasureDest, Color.White);

            //String times = "Real Time: " + String.Format("{0:0.##}", realTime / 1000.0f) + " ModTime: " + String.Format("{0:0.##}", modifiedTime / 1000.0f);
            string times = "Time is going at: " + String.Format("{0:0.##}", timeSpeed) + "x";
            string remainTime = "Remaining time: " + String.Format("{0:0.##}", (levelTime - modifiedTime)/1000.0f) + "s";
            spriteBatch.DrawString(font, "Score: " + score, new Vector2(20, (int)((boundingBox.Y + boundingBox.Bottom) / 2)), Color.Yellow);
            spriteBatch.DrawString(font, times, new Vector2(20, (int)((boundingBox.Y + boundingBox.Bottom + 60) / 2)), Color.Yellow);
            spriteBatch.DrawString(font, remainTime, new Vector2(20, (int)((boundingBox.Y + boundingBox.Bottom + 120) / 2)), Color.Yellow);
            spriteBatch.DrawString(font, "Treasure: " + treasureCount, new Vector2(boundingBox.Right - 445, boundingBox.Bottom - 50), Color.Yellow);
            spriteBatch.DrawString(font, "Junk: " + junkCount, new Vector2(boundingBox.Right - 200, boundingBox.Bottom - 50), Color.Yellow);
        }

        public string storeItem(RiverObject obj)
        {
            if (obj.getObjectType() == RiverObject.ObjectType.Treasure)
            {
                treasureCount++;
                // NOTE: This was supposed to be using visibility, but visibility is scrapped as a feature
                score += 50 + 10 * (int)(timeSpeed * 10);
                return "Treasure visibility: " + visibility + " speed: " + timeSpeed + " scoreadd: " + (50 + 10 * (int)(visibility * 10)) + " score: " + score;
            }
            else
            {
                junkCount++;
                return "Junk NoScore score: " + score;
            }
        }

        public float getModTime()
        {
            return modifiedTime;
        }

        public float getRealTime()
        {
            return realTime;
        }

        public bool isItemOverInventory(RiverObject obj)
        {
            return boundingBox.Contains(obj.getBoundBox().X, obj.getBoundBox().Y);
        }
    }
}
