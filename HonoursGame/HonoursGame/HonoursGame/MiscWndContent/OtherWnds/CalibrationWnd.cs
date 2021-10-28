using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class CalibrationWnd : WndHandle
    {
        public static string[] speechStrings = { "Next" };

        //private WndType nextWnd;
        private int curFrame;
        private int endFrame;
        private Texture2D background;
        private Rectangle backgroundDest;
        private SpriteFont font;

        private float calibrationTimer;
        private float updateTimer;
        private CalibratorTile tile;
        //private ScaledBCI brainInput;
        private float lastInput;
        private float curInput;
        private float relativePosition;
        private float nextRelativePosition;

        private string minText, maxText;
        private bool cMin, cMax;

        public CalibrationWnd(int wndWidth, int wndHeight, Game1 appRef)
            : base(WndType.Calibration, wndWidth, wndHeight, appRef)
        {
            minText = "Min: Uncalibrated";
            maxText = "Max: Uncalibrated";
            cMax = cMin = false;

            curFrame = 0;
            endFrame = 3;

            background = appRef.Content.Load<Texture2D>("TilePuzzle\\tilebackground");
            backgroundDest = new Rectangle(0, 0, wndWidth, wndHeight);

            tile = new CalibratorTile(wndWidth, wndHeight, appRef);

            font = appRef.Content.Load<SpriteFont>("hugeFont");
            relativePosition = nextRelativePosition = 0.5f;
        }

        public override void update(GameTime gameTime)
        {
            debugSpeechInputGen();

            if(curFrame >= 1)
            {
                calibrationTimer -= gameTime.ElapsedGameTime.Milliseconds;
                if (calibrationTimer < 0)
                {
                    if (!cMax && inputManager.getCalibrator().foundMax())
                    {
                        maxText = "Max: " + inputManager.getCalibrator().getMax();
                        cMax = true;
                    }

                    if (!cMin && inputManager.getCalibrator().foundMin())
                    {
                        minText = "Min: " + inputManager.getCalibrator().getMin();
                        cMin = true;
                    }

                    calibrationTimer = 0;
                }

                updateTimer -= gameTime.ElapsedGameTime.Milliseconds;
                if (updateTimer < 0)
                {
                    updateTimer = 1000;

                    relativePosition = nextRelativePosition;
                    curInput = inputManager.getAlphaState();
                    if (curFrame != 2 && curInput > lastInput)
                    {
                        // relaxed
                        nextRelativePosition += 0.07f;
                        if (nextRelativePosition > 1) nextRelativePosition = 1;
                    }
                    else if (curFrame != 1 && curInput < lastInput)
                    {
                        // concentrating
                        nextRelativePosition -= 0.07f;
                        if (nextRelativePosition < 0) nextRelativePosition = 0;
                    }

                    lastInput = curInput;
                }

                tile.setRelativeLocation(relativePosition + (nextRelativePosition - relativePosition) * (1000 - updateTimer) / 1000.0f);
            }
            /*else if (curFrame == 3)
            {
                tile.setRelativeLocation(brainInput.getScaledValue());
            }*/
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, backgroundDest, Color.White);

            tile.draw(spriteBatch);

            spriteBatch.DrawString(font, minText,
                                            new Vector2(10, wndHeight - 70),
                                            Color.Red);
            spriteBatch.DrawString(font, maxText,
                                            new Vector2(10, wndHeight - 40),
                                            Color.Red);

            if(curFrame == 0 || curFrame == 3)
            {
                spriteBatch.DrawString(font, "Concentrate!",
                                            new Vector2(wndWidth / 2 - 95, 50 + 10),
                                            Color.Red);
                spriteBatch.DrawString(font, "Relax!",
                                            new Vector2(wndWidth/2 - 45, 518 + 10),
                                            Color.Red);
                spriteBatch.DrawString(font, "(wait for further instruction)",
                                            new Vector2(wndWidth / 2 - 210, wndHeight / 2),
                                            Color.Red);
            }
            else if (curFrame == 1)
            {
                spriteBatch.DrawString(font, "Relax!",
                                            new Vector2(wndWidth / 2 - 45, 518 + 10),
                                            Color.Red);
                spriteBatch.DrawString(font, (calibrationTimer / 1000.0f).ToString("0.0"), new Vector2(wndWidth - 100, wndHeight - 50), Color.Red);
            }
            else if (curFrame == 2)
            {
                spriteBatch.DrawString(font, "Concentrate!",
                                            new Vector2(wndWidth / 2 - 95, 50 + 10),
                                            Color.Red);
                spriteBatch.DrawString(font, (calibrationTimer / 1000.0f).ToString("0.0"), new Vector2(wndWidth - 100, wndHeight - 50), Color.Red);
            }
            else if (curFrame == 3)
            {
                spriteBatch.DrawString(font, "Practice!",
                                            new Vector2(wndWidth / 2 - 68, wndHeight / 2),
                                            Color.Red);
            }

        }

        public void debugSpeechInputGen()
        {
            if (inputManager.isKeyPressed(Microsoft.Xna.Framework.Input.Keys.D5)
                || inputManager.isBtnPressed(Microsoft.Xna.Framework.Input.Buttons.A))
            {
                if (curFrame == endFrame)
                    appRef.setInfoWnd(WndType.TilePuzzle);
                else
                    curFrame++;

                if (curFrame == 1)
                {
                    inputManager.beginCalibration(Calibrator.CalibrateMode.CalibrateMax);
                    calibrationTimer = 20000;
                    updateTimer = 0;
                    nextRelativePosition = relativePosition = 0.5f;
                    lastInput = inputManager.getAlphaState();
                }
                else if (curFrame == 2)
                {
                    inputManager.beginCalibration(Calibrator.CalibrateMode.CalibrateMin);
                    lastInput = inputManager.getAlphaState();
                    calibrationTimer = 20000;
                    updateTimer = 0;
                    nextRelativePosition = relativePosition = 0.5f;
                }
                else if (curFrame == 3)
                {
                    Calibrator c = inputManager.getCalibrator();
                    appRef.insertLog(DataLog.DataType.Brain, DataElement.DataType.BCI, 
                                        "Calibration Completed to use values. Min: " + c.getMin() + " Max: " + c.getMax());
                    /*brainInput = new ScaledBCI(inputManager, appRef, -1, -1, 0.5f, 0, 1, -0.1f, -0.05f, 0.01f, 0.005f);
                    Rectangle scaledBCIDest = new Rectangle(15, (wndHeight - 200) / 2, 75, 200);
                    Rectangle scaledBCIProgressDest = new Rectangle(scaledBCIDest.X + (75 - 37) / 2 - 1, scaledBCIDest.Y + (200 - 159) / 2, 37, 159);
                    brainInput.configGraphics(scaledBCIDest, scaledBCIProgressDest, false);*/
                    nextRelativePosition = relativePosition = 0.5f;
                }
                //handleSpeechRecognised(speechStrings[0]);
            }
            else if (inputManager.isKeyPressed(Microsoft.Xna.Framework.Input.Keys.D8)
                || inputManager.isBtnPressed(Microsoft.Xna.Framework.Input.Buttons.A))
            {
                // reset button for recalibration
                curFrame = 0;
                nextRelativePosition = relativePosition = 0.5f;
                tile.setRelativeLocation(relativePosition + (nextRelativePosition - relativePosition) * (1000 - updateTimer) / 1000.0f);
                inputManager.getCalibrator().clearCalibration();
                cMin = cMax = false;
                minText = "Min: Uncalibrated";
                maxText = "Max: Uncalibrated";
            }
        }

        public override void handleSpeechRecognised(string s)
        {
            return;

            /*int resultID = -1;
            for (int i = 0; i < speechStrings.Length; i++)
            {
                if (s.Equals(speechStrings[i]))
                {
                    resultID = i;
                    break;
                }
            }

            if (resultID == -1)
            {
                return;
            }

            if (resultID == 0)
            {
                if (curFrame == endFrame)
                    appRef.setInfoWnd(WndType.TilePuzzle);
                else
                    curFrame++;
            } */
        }
    }
}
