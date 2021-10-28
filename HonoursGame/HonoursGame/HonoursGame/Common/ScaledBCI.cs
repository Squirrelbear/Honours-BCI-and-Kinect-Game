using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class ScaledBCI
    {
        private int stateID, subStateID;
        private InputManager inputManager;
        private Game1 appRef;

        //private int history;
        private float curValue, nextValue;

        private float minValue, maxValue;
        private float increment, decrement;
        private float thresholdIncrement, thresholdDecrement;
        
        private float timer;

        private float thresholdIncMod, thresholdDecMod;
        private float thresholdIncEnd, thresholdDecEnd;
        private float timerIncMod, timerDecMod;
        private float incTimeSpan, decTimeSpan;

        private Texture2D foregroundSprite;
        private Texture2D progressSprite;
        private Rectangle dest, progressDest;
        private Rectangle actualProgressDest;
        private bool graphicsEnabled;
        private bool horizontal;

        public ScaledBCI(InputManager inputManager, Game1 appRef, int stateID, int subStateID, float startValue, float minValue, float maxValue, float increment, float decrement, float thresholdIncrement, float thresholdDecrement)
        {
            this.appRef = appRef;
            this.inputManager = inputManager;
            this.stateID = stateID;
            this.subStateID = subStateID;
            this.minValue = minValue;
            this.maxValue = maxValue;

            this.increment = increment;
            this.decrement = decrement;
            this.thresholdDecrement = thresholdDecrement;
            this.thresholdIncrement = thresholdIncrement;

            //history = 0;
            curValue = startValue;
            nextValue = startValue;
            timer = 0;

            thresholdIncMod = thresholdIncEnd = thresholdDecMod = thresholdDecEnd = 0;
            timerDecMod = timerIncMod = 0;
            incTimeSpan = decTimeSpan = -1;

            graphicsEnabled = false;
        }

        public void configThresholdMod(float thresholdIncMod, float thresholdDecMod, float thresholdIncEnd, float thresholdDecEnd, float incTimeSpan, float decTimeSpan)
        {
            this.thresholdIncMod = thresholdIncMod;
            this.thresholdIncEnd = thresholdIncEnd;
            this.thresholdDecMod = thresholdDecMod;
            this.thresholdDecEnd = thresholdDecEnd;
            this.timerDecMod = decTimeSpan;
            this.timerIncMod = incTimeSpan;
            this.incTimeSpan = incTimeSpan;
            this.decTimeSpan = decTimeSpan;
        }

        public void configGraphics(Rectangle dest, Rectangle progressDest)
        {
            configGraphics(dest, progressDest, true);
        }

        public void configGraphics(Rectangle dest, Rectangle progressDest, bool horizontal)
        {
            this.horizontal = horizontal;

            string spriteCode = (horizontal) ? "Hor" : "Ver";
            foregroundSprite = appRef.Content.Load<Texture2D>("Common\\progressFG" + spriteCode);
            progressSprite = appRef.Content.Load<Texture2D>("Common\\progressBG" + spriteCode);

            this.dest = dest;
            this.progressDest = progressDest;
            if(horizontal)
                this.actualProgressDest = new Rectangle(progressDest.X, progressDest.Y, 0, progressDest.Height);
            else
                this.actualProgressDest = new Rectangle(progressDest.X, progressDest.Y + progressDest.Height, progressDest.Width, 0);
            graphicsEnabled = true;
        }

        public void update(GameTime gameTime)
        {
            timer -= gameTime.ElapsedGameTime.Milliseconds;

            if (incTimeSpan != -1)
                timerIncMod -= gameTime.ElapsedGameTime.Milliseconds;
            if (decTimeSpan != -1)
                timerDecMod -= gameTime.ElapsedGameTime.Milliseconds;

            updateThreshold();
            updateValue();

            if (graphicsEnabled)
                updateGraphics();
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (!graphicsEnabled) return;

            spriteBatch.Draw(progressSprite, actualProgressDest, Color.White);
            spriteBatch.Draw(foregroundSprite, dest, Color.White);
        }

        private void updateValue()
        {
            if (timer <= 0)
            {
                timer = 1000;
                curValue = nextValue;

                float bciState = getCurState();
                if (bciState > thresholdIncrement)
                {
                    nextValue += increment;
                    if (increment >= 0 && nextValue >= maxValue)
                    {
                        nextValue = maxValue;
                    }
                    else if (increment < 0 && nextValue <= minValue)
                    {
                        nextValue = minValue;
                    }
                    appRef.insertLog(DataLog.DataType.Brain, DataElement.DataType.Misc, "BCI Increased " + nextValue + " Actual State: " + bciState + " Threshold: " + thresholdIncrement);
                }
                else if (bciState < thresholdDecrement)
                {
                    nextValue -= decrement;
                    if (decrement <= 0 && nextValue >= maxValue)
                    {
                        nextValue = maxValue;
                    }
                    else if (decrement > 0 && nextValue <= minValue)
                    {
                        nextValue = minValue;
                    }
                    appRef.insertLog(DataLog.DataType.Brain, DataElement.DataType.Misc, "BCI Decreased " + nextValue + " Actual State: " + bciState + " Threshold: " + thresholdDecrement);
                }
                else
                {
                    appRef.insertLog(DataLog.DataType.Brain, DataElement.DataType.Misc, "BCI Unchanged Actual State: " + bciState);
                }
            }
        }

        private void updateThreshold()
        {
            if (incTimeSpan != -1)
            {
                if (timerIncMod <= 0)
                {
                    timerIncMod = incTimeSpan;
                    thresholdIncrement += thresholdIncMod;

                    if (thresholdIncMod < 0)
                    {
                        if (thresholdIncrement < thresholdIncEnd)
                        {
                            thresholdIncrement = thresholdIncEnd;
                        }
                    }
                    else
                    {
                        if (thresholdIncrement > thresholdIncEnd)
                        {
                            thresholdIncrement = thresholdIncEnd;
                        }
                    }

                    appRef.insertLog(DataLog.DataType.Brain, DataElement.DataType.Misc, "Threshold Changed (Increment): " + thresholdIncrement);
                }
            }

            if (decTimeSpan != -1)
            {
                if (timerDecMod <= 0)
                {
                    timerDecMod = decTimeSpan;
                    thresholdDecrement += thresholdDecMod;

                    if (thresholdDecMod < 0)
                    {
                        if (thresholdDecrement < thresholdDecEnd)
                        {
                            thresholdDecrement = thresholdDecEnd;
                        }
                    }
                    else
                    {
                        if (thresholdDecrement > thresholdDecEnd)
                        {
                            thresholdDecrement = thresholdDecEnd;
                        }
                    }

                    appRef.insertLog(DataLog.DataType.Brain, DataElement.DataType.Misc, "Threshold Changed (Decrement): " + thresholdDecrement);
                }
            }
        }

        private void updateGraphics()
        {
            if(horizontal)
                actualProgressDest.Width = (int)(progressDest.Width * getRescaledValue());
            else {
                actualProgressDest.Y = (int)(progressDest.Y + progressDest.Height * (1 - getRescaledValue()));
                actualProgressDest.Height = (int)(progressDest.Height * getRescaledValue());
            }
        }

        public float getCurState()
        {
            if (stateID == -1 && subStateID == -1)
            {
                return inputManager.getAlphaState();
            }
            else if (stateID == -1 && subStateID == -2)
            {
                return inputManager.getThetaState();
            }
            else
            {
                return inputManager.getAlphaStateByID(subStateID, subStateID);
            }
        }

        public float getScaledValue()
        {
            return curValue + (nextValue - curValue) * (1000 - timer) / 1000.0f;
        }

        public float getRescaledValue()
        {
            return (getScaledValue() - minValue) / (maxValue - minValue);
        }
    }
}
