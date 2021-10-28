using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class Thief : AnimatedObj
    {
        public static int SPRITEWIDTH = 256;//128;
        public static int SPRITEHEIGHT = 180;//128;
        public static int HEIGHT = 40;
        public static int WIDTH = 40;

        private bool stopped;
        private float stoppedCooldown;
        private float yPos;

        private bool levelEndReached;

        private bool alerted;
        // how long till the alert can disappear
        private float alertCooldown;
        // how long till next opacity reset (used to modify opacity as % of total time)
        private float alertFlashCooldown;
        Texture2D alertSprite;
        // counter to determine level failed
        private float alertedTime;

        private float flashCooldownTime = 1000;
        private float alertCooldownTime = 5000;

        private int lane;
        private int rowStartX;
        private int colWidth;

        private int nextPathID;

        private StreetPuzzle puzzle;

        public Thief(int rowStartX, int colWidth, Point startPoint, Game1 appRef, StreetPuzzle puzzle)
            : base(appRef.Content.Load<Texture2D>("StreetPuzzle//charblue"), SPRITEWIDTH, SPRITEHEIGHT,
                   new Rectangle(startPoint.X, startPoint.Y, 40, 40))
        {
            this.puzzle = puzzle;
            alerted = false;
            alertCooldown = alertFlashCooldown = -1;
            stopped = false;
            stoppedCooldown = -1;
            alertSprite = appRef.Content.Load<Texture2D>("StreetPuzzle//exclamation");
            alertedTime = 0;

            levelEndReached = false;

            this.rowStartX = rowStartX;
            this.colWidth = colWidth;
            nextPathID = 0;
            setLane(1);
            start();
        }

        public void update(GameTime gameTime, float unmodifiedY, float displaceY)
        {
            base.update(gameTime);

            if (levelEndReached)
            {
                yPos += displaceY;
                dest.Y = (int)yPos;

                dest.X += (int)(100 * gameTime.ElapsedGameTime.Milliseconds / 1000.0f);
                return;
            }

            if (alerted)
            {
                alertCooldown -= gameTime.ElapsedGameTime.Milliseconds;
                alertFlashCooldown -= gameTime.ElapsedGameTime.Milliseconds;
                alertedTime += gameTime.ElapsedGameTime.Milliseconds;

                if (alertFlashCooldown <= 0)
                {
                    alertFlashCooldown = flashCooldownTime;
                }
                if (alertCooldown <= 0)
                {
                    alerted = false;
                    alertedTime = 0;
                    puzzle.getAppRef().insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, 
                        "Thief Alert Ended: Alerted for " + alertedTime / 1000.0f + " seconds.");
                }

                if (alertedTime > 15000)
                {
                    puzzle.getAppRef().insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc,
                        "Thief Alert Ended Level Failed.");
                    puzzle.failLevel("You remained too close to the target too long!\nLook out for the warning symbol above their head.");
                }
            }

            if (stopped)
            {
                yPos += displaceY;
                dest.Y = (int)yPos;
                stoppedCooldown -= gameTime.ElapsedGameTime.Milliseconds;
                if (stoppedCooldown <= 0)
                {
                    start();
                }
            }
            else
            {
                yPos -= (unmodifiedY - displaceY);
                dest.Y = (int)yPos;
            }
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            base.draw(spriteBatch);

            if (alerted)
            {
                Rectangle alertDest = new Rectangle(dest.X + (int)(WIDTH / 4.0f) / 2, dest.Y - HEIGHT, (int)(WIDTH / 4.0f * 3), (int)(HEIGHT / 4.0f * 3));
                //Console.WriteLine("Value: " + (alertFlashCooldown / flashCooldownTime));
                spriteBatch.Draw(alertSprite, alertDest, Color.White * (alertFlashCooldown / flashCooldownTime));
            }
        }

        public bool testAlerted(Rectangle playerRect)
        {            
            return getDistanceFrom(playerRect) < 150;
        }

        public float getDistanceFrom(Rectangle playerRect)
        {
            Vector2 playerCentre = new Vector2(playerRect.X + playerRect.Width / 2, playerRect.Y + playerRect.Height / 2);
            Vector2 thisCentre = new Vector2(dest.X + dest.Width / 2, dest.Y + dest.Height / 2);

            float distance = Vector2.Distance(playerCentre, thisCentre);

            return distance;
        }

        public void resetAlerted()
        {
            if (!alerted)
            {
                puzzle.getAppRef().insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc,
                        "Thief Alert Started!");
            }

            alerted = true;
            alertCooldown = alertCooldownTime;
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

        public void stop(float stopTime)
        {
            stopped = true;
            stoppedCooldown = stopTime;
            yPos = dest.Y;
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

        public int getNextPathID()
        {
            return nextPathID;
        }

        public void nextPath()
        {
            nextPathID++;
        }

        public bool hasReachedLevelEnd()
        {
            return levelEndReached;
        }

        public void reachedLevelEnd()
        {
            if (!levelEndReached)
            {
                setRotation(MathHelper.PiOver2);
            }

            levelEndReached = true;
        }
    }
}
