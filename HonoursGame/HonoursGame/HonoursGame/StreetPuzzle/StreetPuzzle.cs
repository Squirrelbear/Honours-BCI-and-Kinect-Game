using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class StreetPuzzle : WndHandle
    {
        public static string[] speechStrings = { "pause game", "confirm", "change hands", "swap hands" };
        public enum SelectModeAction { None, Select };

        //private float timeMultiplier;
        private float speedMultiplier;
        private List<ObstacleObj> obstacles;
        private List<ObstacleObj> removeObstacles;
        private PlayerObj player;
        private Thief thief;
        private StreetMap map;

        private int OBSTACLEWIDTH = 128 / 2;
        private int OBSTACLEHEIGHT = 128 / 2;
        private int ROWSTARTX = 256;
        // distance per second based on no speed multiplier
        private float BASESPEED = 100;
        private float nextRowInsertY;

        private float hipInputDivision;
        private float hipInputThreshold;

        private string statusString;
        private SpriteFont font;

        // http://texturelib.com/texture/?path=/Textures/ground/stone%20ground/ground_stone_ground_0012
        private Texture2D backgroundSprite;
        private Rectangle destBackground, destBackgroundAlt, sourceBackground;
        private float backgroundY, backgroundYAlt;
        private float backgroundHeight;

        // Scrolling Buildings
        private Texture2D buildingSprite;
        private Rectangle destBuildings, destBuildingsAlt, sourceBuildings;
        private Rectangle destBuildings2, destBuildingsAlt2;
        private float buildingsY, buildingsYAlt;
        private float buildingsHeight;

        // Scrolling Water
        private Texture2D waterSprite;
        private Rectangle destWater, destWaterAlt, sourceWater;
        private float waterY, waterYAlt;
        private float waterHeight;

        private WndHandle overlayWnd;
        private SelectModeAction selectModeAction;
        private Cursor cursor;
        private Texture2D cursorSprite;
        private Texture2D cursorProgressSprite;
        private int remainingAttempts;

        private InputManager.HandInputMode primaryHand;

        private ScaledBCI bciInput;

        public StreetPuzzle(int remainingAttempts, int wndWidth, int wndHeight, Game1 appRef)
            : base(WndType.StreetPuzzle, wndWidth, wndHeight, appRef)
        {
            appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventPuzzleStart, "Street Puzzle Started");
            this.remainingAttempts = remainingAttempts;
            //timeMultiplier = 1.0f;
            speedMultiplier = 1.0f;
            obstacles = new List<ObstacleObj>();
            player = new PlayerObj(ROWSTARTX, OBSTACLEWIDTH, appRef, new Point(wndWidth / 2, wndHeight - 256));
            thief = new Thief(ROWSTARTX, OBSTACLEWIDTH, new Point(wndWidth / 2, 256), appRef, this);
            map = new StreetMap(appRef);
            removeObstacles = new List<ObstacleObj>();

            // create rows equal to the number of elements to fill vertically
            for (nextRowInsertY = -OBSTACLEHEIGHT; nextRowInsertY > -wndHeight; nextRowInsertY -= OBSTACLEHEIGHT)
            {
                addObstacles(map.getNextRow(ROWSTARTX, (int)nextRowInsertY, OBSTACLEWIDTH, OBSTACLEHEIGHT));
            }

            // NOTE: Dummy fix for issue that added 2 rows of empty space only after the first rows insertion
            nextRowInsertY += 2 * OBSTACLEHEIGHT;

            /*for (nextRowInsertY = -OBSTACLEHEIGHT; !map.allRowsRetrieved(); nextRowInsertY -= OBSTACLEHEIGHT)
            {
                addObstacles(map.getNextRow(ROWSTARTX, (int)nextRowInsertY, OBSTACLEWIDTH, OBSTACLEHEIGHT));
            }*/

            hipInputDivision = (wndWidth-200) / 5;
            hipInputThreshold = (wndWidth-200) / 8;
            /*Console.WriteLine("0 (+): " + (hipInputDivision * 0 + hipInputThreshold));
            Console.WriteLine("1 (+): " + (hipInputDivision * 1 + hipInputThreshold));
            Console.WriteLine("1 (-): " + (hipInputDivision * 1 - hipInputThreshold));
            Console.WriteLine("2 (-): " + (hipInputDivision * 2 - hipInputThreshold));*/
            inputManager.setUseHipCentre(true);
            primaryHand = inputManager.getHandInputMode();
            inputManager.setHandInputMode(InputManager.HandInputMode.Hand_Both);
            inputManager.setUseShoulderCentre(true);

            // Setup primary background
            backgroundSprite = appRef.Content.Load<Texture2D>("StreetPuzzle//background");
            sourceBackground = new Rectangle(0, 0, backgroundSprite.Width, backgroundSprite.Height);
            float backgroundScale = wndWidth * 1.0f / backgroundSprite.Width;
            backgroundHeight = (backgroundSprite.Height * backgroundScale);
            destBackground = new Rectangle(0, 0, wndWidth, (int)backgroundHeight);
            destBackgroundAlt = new Rectangle(0, -(int)backgroundHeight, wndWidth, (int)backgroundHeight);
            backgroundY = 0;
            backgroundYAlt = -(int)backgroundHeight;

            // Setup buildings
            buildingSprite = appRef.Content.Load<Texture2D>("StreetPuzzle//rooftops");
            sourceBuildings = new Rectangle(0, 0, buildingSprite.Width, buildingSprite.Height);
            float buildingScale = 0.8f;
            buildingsHeight = buildingSprite.Height * buildingScale;
            destBuildings = new Rectangle(ROWSTARTX - (int)(buildingSprite.Width * buildingScale), 0, (int)(buildingSprite.Width * buildingScale), (int)buildingsHeight);
            destBuildingsAlt = new Rectangle(ROWSTARTX - (int)(buildingSprite.Width * buildingScale), -(int)buildingsHeight, (int)(buildingSprite.Width * buildingScale), (int)buildingsHeight);
            destBuildings2 = new Rectangle(ROWSTARTX + OBSTACLEWIDTH * 5, 0, (int)(buildingSprite.Width * buildingScale), (int)buildingsHeight);
            destBuildingsAlt2 = new Rectangle(ROWSTARTX + OBSTACLEWIDTH * 5, -(int)buildingsHeight, (int)(buildingSprite.Width * buildingScale), (int)buildingsHeight);
            buildingsY = 0;
            buildingsYAlt = -(int)buildingsHeight;

            /*// Setup water upward movement
            waterSprite = appRef.Content.Load<Texture2D>("StreetPuzzle//scrollingwater");
            sourceWater = new Rectangle(0, 0, waterSprite.Width, waterSprite.Height);
            waterHeight = waterSprite.Height;
            destWater = new Rectangle(ROWSTARTX + OBSTACLEWIDTH * 5 + (int)(waterSprite.Width * buildingScale), 0,
                                     (int)(waterSprite.Width), (int)waterHeight);
            destWaterAlt = new Rectangle(ROWSTARTX + OBSTACLEWIDTH * 5 + (int)(waterSprite.Width * buildingScale), (int)waterHeight, 
                                    (int)(waterSprite.Width), (int)waterHeight);
            waterY = 0;
            waterYAlt = waterHeight;*/

            // Setup water downward movement
            waterSprite = appRef.Content.Load<Texture2D>("StreetPuzzle//scrollingwater");
            sourceWater = new Rectangle(0, 0, waterSprite.Width, waterSprite.Height);
            waterHeight = waterSprite.Height;
            destWater = new Rectangle(ROWSTARTX + OBSTACLEWIDTH * 5 + (int)(waterSprite.Width * buildingScale), 0,
                                     (int)(waterSprite.Width), (int)waterHeight);
            destWaterAlt = new Rectangle(ROWSTARTX + OBSTACLEWIDTH * 5 + (int)(waterSprite.Width * buildingScale), (int)(-waterHeight),
                                    (int)(waterSprite.Width), (int)waterHeight);
            waterY = 0;
            waterYAlt = -waterHeight;

            font = appRef.Content.Load<SpriteFont>("smallFont");
            statusString = "";

            overlayWnd = null;
            cursorSprite = appRef.Content.Load<Texture2D>("TilePuzzle\\hand");
            cursorProgressSprite = appRef.Content.Load<Texture2D>("Common\\progress");
            cursor = new Cursor(cursorSprite, cursorProgressSprite);

            bciInput = new ScaledBCI(inputManager, appRef, -1, -1, 0.9f, 0.5f, 1.3f, -0.05f, -0.1f, 0.4f, 0.35f);

            //bciInput.configThresholdMod(-0.075f, 0.075f, 0.14f, 0.9f, 500, 500);

            Rectangle scaledBCIDest = new Rectangle(15, (wndHeight - 200) / 2, 75, 200);
            Rectangle scaledBCIProgressDest = new Rectangle(scaledBCIDest.X + (75 - 37) / 2 - 1, scaledBCIDest.Y + (200 - 159) / 2, 37, 159);
            bciInput.configGraphics(scaledBCIDest, scaledBCIProgressDest, false);
        }

        public override void update(GameTime gameTime)
        {
            
            if (primaryHand == InputManager.HandInputMode.Hand_Right)
            {
                cursor.setPoint(inputManager.getHandPosition(true, true));
            }
            else
            {
                cursor.setPoint(inputManager.getHandPosition(false, true));
            }
            if (appRef.getEnableDebugInput())
            {
                debugSpeechInputGen();
            }
            cursor.update(gameTime);

            if (overlayWnd != null)
            {
                if (cursor.isSelectionComplete())
                {
                    if (selectModeAction == SelectModeAction.Select)
                    {
                        if (overlayWnd.getWndType() == WndType.ConfirmDialog)
                        {
                            if (((ConfirmDialog)overlayWnd).confirmAction())
                            {
                                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventPuzzleEnd, "Early Termination To Main Menu!");
                                appRef.setWnd(WndType.MainMenu);
                            }
                            else
                            {
                                overlayWnd = null;
                                inputManager.setHandInputMode(InputManager.HandInputMode.Hand_Both);
                                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Confirm Dialog Closed");
                            }
                        }
                        else if (overlayWnd.getWndType() == WndType.VictoryDialog)
                        {
                            WndType result = ((VictoryDialog)overlayWnd).getAction();
                            if (result == WndType.MainMenu)
                            {
                                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventPuzzleEnd, "Early Termination To Main Menu!");
                                appRef.setWnd(WndType.MainMenu);
                            }
                            else
                            {
                                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventPuzzleEnd, "Successful Termination");
                                appRef.setInfoWnd(WndType.RiverPuzzle);
                            }
                        }
                        else if (overlayWnd.getWndType() == WndType.FailureDialog)
                        {
                            WndType result = ((FailureDialog)overlayWnd).getAction();
                            if (result == WndType.RiverPuzzle)
                            {
                                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventPuzzleEnd, "Level failed. Too many attempts made. Forced to next level.");
                                appRef.setInfoWnd(WndType.RiverPuzzle);
                            }
                            else
                            {
                                // will either call for main menu or force a reset of the level
                                if (result == WndType.MainMenu)
                                {
                                    appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventPuzzleEnd, "Early Termination To Main Menu!");
                                }
                                else
                                {
                                    appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventPuzzleEnd, "Level reattempt initiated.");
                                }
                                appRef.setWnd(result);
                            }
                        }
                    }

                    selectModeAction = SelectModeAction.None;
                    cursor.clearSelectionProgress();
                }
                else if (!cursor.isSelecting())
                {
                    selectModeAction = SelectModeAction.None;
                    if (overlayWnd != null)
                    {
                        if (overlayWnd.getWndType() == WndType.ConfirmDialog)
                        {
                            ((ConfirmDialog)overlayWnd).resetSelection();
                        }
                        else if (overlayWnd.getWndType() == WndType.VictoryDialog)
                        {
                            ((VictoryDialog)overlayWnd).resetSelection();
                        }
                        else
                        {
                            ((FailureDialog)overlayWnd).resetSelection();
                        }
                    }
                }

                // skip any further updates this pass
                if (overlayWnd == null) return;

                if (overlayWnd.getWndType() == WndType.ConfirmDialog)
                {
                    ((ConfirmDialog)overlayWnd).updateCursor(cursor.getPoint());
                }
                else if (overlayWnd.getWndType() == WndType.VictoryDialog)
                {
                    ((VictoryDialog)overlayWnd).updateCursor(cursor.getPoint());
                }
                else if (overlayWnd.getWndType() == WndType.FailureDialog)
                {
                    ((FailureDialog)overlayWnd).updateCursor(cursor.getPoint());
                }

                overlayWnd.update(gameTime);
                return;
            }

            bool addNewRow = false;
            float basicTranslation = BASESPEED * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            float updateYMod = speedMultiplier * basicTranslation;
            nextRowInsertY += updateYMod;
            if (nextRowInsertY >= -OBSTACLEHEIGHT)
            {
                nextRowInsertY -= OBSTACLEHEIGHT;
                addNewRow = true;
            }

            speedMultiplier = 0.0f;

            foreach (ObstacleObj obj in obstacles)
            {
                obj.update(gameTime, updateYMod);

                if (obj.canCollidePlayer() && obj.collidingWith(player.getRect()))
                {
                    if (obj.getType() == ObstacleObj.ObstacleType.LevelEnd)
                    {
                        // level complete
                        appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventPuzzleEnd, "Successful completion of puzzle.");
                        overlayWnd = new VictoryDialog("You successfully followed the thief!", WndType.RiverPuzzle,
                                                        wndWidth, wndHeight, appRef);
                        inputManager.setHandInputMode(primaryHand);
                    }
                    else
                    {
                        speedMultiplier = -0.2f;
                        if (!obj.isCollided())
                        {
                            appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Collided with an obstacle.");
                        }
                        obj.triggerCollided();
                    }
                }
                else if (obj.getType() == ObstacleObj.ObstacleType.Path 
                          || obj.getType() == ObstacleObj.ObstacleType.PathStop
                          || obj.getType() == ObstacleObj.ObstacleType.LevelEnd) 
                {
                    PathHelper tmp = (PathHelper)obj;
                    if ( (tmp.getPathID() == thief.getNextPathID() || obj.getType() == ObstacleObj.ObstacleType.LevelEnd)
                        && tmp.canTriggerMove(thief.getRect().Y, thief.getRect().Height))
                    {
                        if (tmp.getType() == ObstacleObj.ObstacleType.PathStop)
                        {
                            if (!tmp.isStopHandled())
                            {
                                thief.stop(2000);
                                tmp.handleStop();
                                continue;
                            }
                            else if (thief.getStopped())
                            {
                                continue;
                            }
                        }
                        else if(tmp.getType() == ObstacleObj.ObstacleType.LevelEnd)
                        {
                            if (!thief.hasReachedLevelEnd())
                            {
                                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Thief reached level end.");
                            }

                            thief.reachedLevelEnd();
                        }
                        
                        if (tmp.getDirection() == PathHelper.Direction.Left)
                        {
                            thief.setLane(thief.getLane() - 1);
                        }
                        else if (tmp.getDirection() == PathHelper.Direction.Right)
                        {
                            thief.setLane(thief.getLane() + 1);
                        }
                        thief.nextPath();
                        tmp.triggerCollided();
                    }
                }

                if (obj.hasExitedScreen(wndHeight))
                {
                    removeObstacle(obj);
                }
            }
            // add multiplier between 0.15 and 0.3
            // BCI SPEED MODIFICATION IS HERE
            bciInput.update(gameTime);
            speedMultiplier += bciInput.getScaledValue();//0.20f * inputManager.getAlphaState() + 0.1f;
            statusString = "Speed: " + speedMultiplier;
            appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Speed: " + speedMultiplier);  

            updatePlayer(gameTime);
            thief.update(gameTime, basicTranslation, updateYMod);

            if (thief.testAlerted(player.getRect()))
            {
                thief.resetAlerted();
            }

            updateBackground(updateYMod, gameTime);

            // clear the elements from the actual obstacle array
            foreach (ObstacleObj obj in removeObstacles)
            {
                obstacles.Remove(obj);
            }
            removeObstacles.Clear();

            // add new obstacles if required
            if (addNewRow && !map.allRowsRetrieved())
            {
                addObstacles(map.getNextRow(ROWSTARTX, (int)nextRowInsertY - OBSTACLEHEIGHT, OBSTACLEWIDTH, OBSTACLEHEIGHT));
            }
            else if (addNewRow && map.allRowsRetrieved())
            {
                // this should add in dummy rows
                addObstacles(map.getNextRow(ROWSTARTX, (int)nextRowInsertY - OBSTACLEHEIGHT, OBSTACLEWIDTH, OBSTACLEHEIGHT));
            }
        }

        public void updatePlayer(GameTime gameTime)
        {
            float handLY = inputManager.getHandPosition(false, false).Y;
            float handRY = inputManager.getHandPosition(true, false).Y;
            float shoulderPos = inputManager.getShoulderCentre().Y;
            //statusString += " handL: " + handLY + " handR: " + handRY + " shoulder: " + shoulderPos;
            appRef.insertLog(DataLog.DataType.Input, DataElement.DataType.InputMisc, "Input Coords: handL: " + handLY + " handR: " + handRY
                              + " shoulder: " + shoulderPos + " hip: " + inputManager.getHipCentre());

            if ((handLY < shoulderPos && handRY < shoulderPos) 
                 || inputManager.isBtnDown(Microsoft.Xna.Framework.Input.Buttons.A) 
                 || inputManager.isKeyDown(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                player.stop();
                speedMultiplier = 0;
                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Player Movement Halted");
            }
            else
            {
                if (player.getStopped())
                {
                    player.start();
                    appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Player Resumed Movement");
                }
            }

            if (!player.getStopped())
            {
                // TODO: FIX THIS....
                float hipPos = (inputManager.getHipCentre() + 100/*+ (1024 / 2.0f) / 5.0f*/);
                if (player.getLane() > 0
                     && hipPos - 220 < (player.getLane()) * hipInputDivision - hipInputThreshold)
                {
                    player.setLane(player.getLane() - 1);
                    appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Moved Player Left to lane: " + player.getLane());
                }
                else if (player.getLane() < 4
                          && hipPos > (player.getLane()+1) * hipInputDivision + hipInputThreshold)
                {
                    player.setLane(player.getLane() + 1);
                    appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Moved Player Right to lane: " + player.getLane());
                }
            }
            player.update(gameTime);
        }

        public void updateBackground(float updateYMod, GameTime gameTime)
        {
            // Update main background
            backgroundY += updateYMod;
            backgroundYAlt += updateYMod;
            if (backgroundY > wndHeight)
            {
                backgroundY = (int)backgroundYAlt - (int)backgroundHeight;
            }
            else if (backgroundYAlt > wndHeight)
            {
                backgroundYAlt = (int)backgroundY - (int)backgroundHeight;
            }
            destBackground.Y = (int)backgroundY;
            destBackgroundAlt.Y = (int)backgroundYAlt;

            // Update rooftop background
            buildingsY += updateYMod;
            buildingsYAlt += updateYMod;
            if (buildingsY > wndHeight)
            {
                buildingsY = (int)buildingsYAlt - (int)buildingsHeight;
            }
            else if (buildingsYAlt > wndHeight)
            {
                buildingsYAlt = (int)buildingsY - (int)buildingsHeight;
            }
            destBuildings.Y = (int)buildingsY;
            destBuildingsAlt.Y = (int)buildingsYAlt;
            destBuildings2.Y = (int)buildingsY;
            destBuildingsAlt2.Y = (int)buildingsYAlt;

            /*// Update water background upward
            waterY -= BASESPEED * gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 0.8f;
            waterYAlt -= BASESPEED * gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 0.8f;
            if (waterY + waterHeight < 0)
            {
                waterY = (int)waterYAlt + (int)waterHeight;
            }
            else if (waterYAlt + waterHeight < 0)
            {
                waterYAlt = (int)waterY + (int)waterHeight;
            }
            destWater.Y = (int)waterY;
            destWaterAlt.Y = (int)waterYAlt;*/

            // Update water background downward
            waterY += updateYMod + BASESPEED * gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 0.4f;
            waterYAlt += updateYMod + BASESPEED * gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 0.4f;
            if (waterY > waterHeight)
            {
                waterY = (int)waterYAlt - (int)waterHeight;
            }
            else if (waterYAlt > waterHeight)
            {
                waterYAlt = (int)waterY - (int)waterHeight;
            }
            destWater.Y = (int)waterY;
            destWaterAlt.Y = (int)waterYAlt;

            if (inputManager.getNoKinect())
            {
                debugSpeechInputGen();
            }
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundSprite, destBackground, sourceBackground, Color.White);
            spriteBatch.Draw(backgroundSprite, destBackgroundAlt, sourceBackground, Color.White);

            foreach(ObstacleObj obj in obstacles)
            {
                obj.draw(spriteBatch);
            }

            thief.draw(spriteBatch);

            spriteBatch.Draw(buildingSprite, destBuildings, sourceBuildings, Color.White);
            spriteBatch.Draw(buildingSprite, destBuildingsAlt, sourceBuildings, Color.White);
            spriteBatch.Draw(buildingSprite, destBuildings2, sourceBuildings, Color.White);
            spriteBatch.Draw(buildingSprite, destBuildingsAlt2, sourceBuildings, Color.White);
            spriteBatch.Draw(waterSprite, destWater, sourceWater, Color.White);
            spriteBatch.Draw(waterSprite, destWaterAlt, sourceWater, Color.White);

            player.draw(spriteBatch);
            spriteBatch.DrawString(font, statusString, new Vector2(2, 30), Color.White);

            bciInput.draw(spriteBatch);

            if (overlayWnd != null)
            {
                overlayWnd.draw(spriteBatch);
                cursor.draw(spriteBatch);
            }
        }

        public void addObstacles(List<ObstacleObj> objs)
        {
            foreach (ObstacleObj obj in objs)
            {
                obstacles.Add(obj);
            }
        }

        public void removeObstacle(ObstacleObj obj)
        {
            // queue the object for safe removal
            removeObstacles.Add(obj);
        }

        public void failLevel(string message)
        {
            appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Puzzle Level Failed Remaining: " + remainingAttempts);
            WndType nextWnd = WndType.RiverPuzzle;
            if(remainingAttempts > 0) 
            {
                nextWnd = WndType.StreetPuzzle;
                message += "\nYou have " + remainingAttempts + " attempt" + ((remainingAttempts > 1) ? "s" : "") + " remaining.";
            } 
            else 
            {
                message += "\nYou have used up all your attempts at this puzzle.";
            }

            overlayWnd = new FailureDialog(message, nextWnd, (remainingAttempts > 0), wndWidth, wndHeight, appRef);
            inputManager.setHandInputMode(primaryHand);
        }

        public void debugSpeechInputGen()
        {
            /*if (inputManager.isKeyPressed(Microsoft.Xna.Framework.Input.Keys.D5)
                || inputManager.isBtnPressed(Microsoft.Xna.Framework.Input.Buttons.A))
            {
                // Start Puzzle
                handleSpeechRecognised(speechStrings[0]);
            }*/
            if (inputManager.isKeyPressed(Microsoft.Xna.Framework.Input.Keys.D6)
                || inputManager.isBtnPressed(Microsoft.Xna.Framework.Input.Buttons.Start))
            {
                // main menu
                handleSpeechRecognised(speechStrings[0]);
            }
            else if (inputManager.isKeyPressed(Microsoft.Xna.Framework.Input.Keys.D5)
                || inputManager.isBtnPressed(Microsoft.Xna.Framework.Input.Buttons.A))
            {

                if (overlayWnd != null)
                {
                    // select button
                    handleSpeechRecognised(speechStrings[1]);
                }
            }
            else if (inputManager.isKeyPressed(Microsoft.Xna.Framework.Input.Keys.D4))
            {
                // swap hands
                handleSpeechRecognised(speechStrings[2]);
            }
        }

        public override void handleSpeechRecognised(string s)
        {
            int resultID = -1;
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
                //statusString = "Voice command not found";
                appRef.insertLog(DataLog.DataType.Input, DataElement.DataType.VoiceRejected, "Voice command not found for specific window. (" + s + ")"); 
                return;
            }

            switch (resultID)
            {
                case 0:
                    if (overlayWnd == null)
                    {
                        appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Opened Confirm Dialog");  
                        overlayWnd = new ConfirmDialog(wndWidth, wndHeight, appRef);
                        inputManager.setHandInputMode(primaryHand);
                    }
                    break;
                case 1:
                    Rectangle selRect;
                    if (overlayWnd == null) break;
                    if (overlayWnd.getWndType() == WndType.ConfirmDialog)
                    {
                        selRect = ((ConfirmDialog)overlayWnd).getSelectionRect(cursor.getPoint());
                    }
                    else if(overlayWnd.getWndType() == WndType.VictoryDialog)
                    {
                        selRect = ((VictoryDialog)overlayWnd).getSelectionRect(cursor.getPoint());
                    }
                    else
                    {
                        selRect = ((FailureDialog)overlayWnd).getSelectionRect(cursor.getPoint());
                    }
                    if (selRect.X != 0)
                    {
                        selectModeAction = SelectModeAction.Select;
                        cursor.beginSelection(500, selRect);
                    }
                    break;
                case 2:
                case 3:
                    if((primaryHand != InputManager.HandInputMode.Hand_Right))
                    {
                        if(overlayWnd != null)
                            inputManager.setHandInputMode(InputManager.HandInputMode.Hand_Right);
                        primaryHand = InputManager.HandInputMode.Hand_Right;
                        //statusString = "Input hand changed to right";
                        appRef.insertLog(DataLog.DataType.Input, DataElement.DataType.InputMisc, "Hands Swapped to Right");
                    } else {
                        if(overlayWnd != null)
                            inputManager.setHandInputMode(InputManager.HandInputMode.Hand_Left);
                        primaryHand = InputManager.HandInputMode.Hand_Left;
                        //statusString = "Input hand changed to left";
                        appRef.insertLog(DataLog.DataType.Input, DataElement.DataType.InputMisc, "Hands Swapped to Left");
                    }
                    break;
            }
        }
    }
}
