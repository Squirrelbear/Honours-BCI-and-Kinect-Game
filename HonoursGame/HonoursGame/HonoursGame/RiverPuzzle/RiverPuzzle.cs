using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class RiverPuzzle : WndHandle
    {
        public static string[] speechStrings = { "pause game", "confirm", "change hands", "swap hands" };

        public enum SelectModeAction { None, Drop, Select };

        private SelectModeAction selectModeAction;
        private Cursor cursor;

        private Texture2D cursorSprite;
        private Texture2D cursorProgressSprite;
        private List<Texture2D> objSprites;
        private Texture2D background;
        private Rectangle backgroundRect;

        // Scrolling Water
        private Texture2D waterSprite;
        private Rectangle destWater, destWaterAlt, sourceWater;
        private float waterX, waterXAlt;
        private float waterWidth;

        private Random gen;

        private string statusString;
        private SpriteFont font;
        private SpriteFont smallFont;

        private List<RiverObject> riverObjects;
        private List<RiverObject> removalQueue;
        private RiverObject selectedObject;
        private float visibility, speed;
        private Inventory inventory;

        private float spawnCooldown;
        private int[] spawnIndexOrder;

        private int missedTreasure;

        private WndHandle overlayWnd;

        private ScaledBCI bciInput;

        public RiverPuzzle(int wndWidth, int wndHeight, Game1 appRef)
            : base(WndType.RiverPuzzle, wndWidth, wndHeight, appRef)
        {
            appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventPuzzleStart, "River Puzzle Started");
            gen = new Random();
            
            background = appRef.Content.Load<Texture2D>("RiverPuzzle\\background");
            backgroundRect = new Rectangle(0, 0, background.Width, background.Height);

            cursorSprite = appRef.Content.Load<Texture2D>("TilePuzzle\\hand");
            cursorProgressSprite = appRef.Content.Load<Texture2D>("Common\\progress");
            font = appRef.Content.Load<SpriteFont>("hugeFont");
            smallFont = appRef.Content.Load<SpriteFont>("smallFont");
            statusString = "";

            this.wndHeight = wndHeight;
            this.wndWidth = wndWidth;
            visibility = 1.0f;
            speed = 1.0f;

            cursor = new Cursor(cursorSprite, cursorProgressSprite);

            objSprites = new List<Texture2D>();
            objSprites.Add(appRef.Content.Load<Texture2D>("RiverPuzzle\\treasure")); 
            objSprites.Add(appRef.Content.Load<Texture2D>("RiverPuzzle\\treasure2"));
            objSprites.Add(appRef.Content.Load<Texture2D>("RiverPuzzle\\tyre"));
            objSprites.Add(appRef.Content.Load<Texture2D>("RiverPuzzle\\rubbishbag"));
            objSprites.Add(appRef.Content.Load<Texture2D>("RiverPuzzle\\tyre")); // Note: unused

            removalQueue = new List<RiverObject>();
            riverObjects = new List<RiverObject>();

            spawnCooldown = 10;
            spawnIndexOrder = new int[5];
            for (int i = 0; i < 5; i++)
            {
                spawnIndexOrder[i] = i;
            }
            inventory = new Inventory((int)(1000 * 60 * 2.5), new Rectangle(0, wndHeight - 200, wndWidth, 200), objSprites[0], objSprites[2], font, this);

            missedTreasure = 0;
            overlayWnd = null;

            // Setup water
            waterSprite = appRef.Content.Load<Texture2D>("RiverPuzzle//scrollingwater");
            sourceWater = new Rectangle(0, 0, waterSprite.Width, waterSprite.Height);
            waterWidth = waterSprite.Width;
            destWater = new Rectangle(0, 0, (int)(waterWidth), (int)waterSprite.Height);
            destWaterAlt = new Rectangle((int)(waterWidth), 0, (int)(waterWidth), (int)waterSprite.Height);
            waterX = 0;
            waterXAlt = waterWidth;

            bciInput = new ScaledBCI(inputManager, appRef, -1, -1, 0.5f, 0.5f, 1.3f, 0.05f, 0.1f, 0.4f, 0.35f);

            //bciInput.configThresholdMod(-0.075f, 0.075f, 0.14f, 0.9f, 500, 500);

            Rectangle scaledBCIDest = new Rectangle((wndWidth - 200) / 2, 5, 200, 75);
            Rectangle scaledBCIProgressDest = new Rectangle(scaledBCIDest.X + (200 - 159) / 2, scaledBCIDest.Y + (75 - 37) / 2, 159, 37);
            bciInput.configGraphics(scaledBCIDest, scaledBCIProgressDest, true);
        }

        public override void update(GameTime gameTime)
        {
            bciInput.update(gameTime);
            speed = bciInput.getScaledValue();//inputManager.getAlphaState();
            if (speed < 0.1)
            {
                speed = 0.1f;
            }
            visibility = 1;// 1 - inputManager.getAlphaState();
            appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Speed: " + speed + " Visibility: " + visibility);

            if (selectedObject != null && !selectedObject.isSelected())
            {
                cursor.updateSelectionBox(selectedObject.getBoundBox());
            }
            else if(selectedObject != null)
            {
                selectedObject.setLocation(cursor.getPoint().X + 30, cursor.getPoint().Y + 30);
            }
            
            updateCursor(gameTime);

            if (overlayWnd != null)
            {
                if (overlayWnd.getWndType() == WndType.ConfirmDialog)
                {
                    ((ConfirmDialog)overlayWnd).updateCursor(cursor.getPoint());
                }
                else if (overlayWnd.getWndType() == WndType.VictoryDialog)
                {
                    ((VictoryDialog)overlayWnd).updateCursor(cursor.getPoint());
                }

                overlayWnd.update(gameTime);
                return;
            }

            foreach (RiverObject obj in riverObjects)
            {
                obj.update(gameTime, visibility, speed);
                if (obj.getRect().X < -100)
                {
                    removalQueue.Add(obj);
                }
            }

            foreach (RiverObject obj in removalQueue)
            {
                if (obj.getObjectType() == RiverObject.ObjectType.Treasure)
                {
                    if (!obj.isFailed())
                    {
                        obj.failObject();
                        missedTreasure++;
                        appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Missed Treasure (" + missedTreasure + ")");
                    }
                }
                else
                {
                    appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Missed Junk");
                }

                riverObjects.Remove(obj);
            }
            removalQueue.Clear();

            spawnUpdate(gameTime);

            updateWater(gameTime);

            inventory.update(gameTime, speed, visibility);
        }

        public void updateCursor(GameTime gameTime)
        {
            if (inputManager.getHandInputMode() == InputManager.HandInputMode.Hand_Right)
            {
                Vector2 handPos = inputManager.getHandPosition(true, true);
                Vector2 unscaledHandPos = inputManager.getHandPosition(true, false);
                cursor.setPoint(handPos);
                appRef.insertLog(DataLog.DataType.Input, DataElement.DataType.HandCoords,
                    "Hand position: x:" + handPos.X + " y: " + handPos.Y
                    + " Unscaled positions.  x:" + unscaledHandPos.X + " y: " + unscaledHandPos.Y);
            }
            else
            {
                Vector2 handPos = inputManager.getHandPosition(false, true);
                Vector2 unscaledHandPos = inputManager.getHandPosition(false, false);
                cursor.setPoint(handPos);
                appRef.insertLog(DataLog.DataType.Input, DataElement.DataType.HandCoords,
                    "Hand position: x:" + handPos.X + " y: " + handPos.Y
                    + " Unscaled positions.  x:" + unscaledHandPos.X + " y: " + unscaledHandPos.Y);
            }
            if (appRef.getEnableDebugInput())
            {
                debugSpeechInputGen();
            }
            cursor.update(gameTime);

            if (cursor.isSelectionComplete())
            {
                if (selectModeAction == SelectModeAction.Select)
                {
                    if (overlayWnd == null)
                    {
                        appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, 
                                            "Selected Object " + selectedObject.getObjectType().ToString());
                        selectedObject.select();
                        riverObjects.Remove(selectedObject);
                    }
                    else
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
                                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Confirm Dialog Closed");
                                overlayWnd = null;
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
                                appRef.setInfoWnd(WndType.StreetPuzzle);
                            }
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
                }
                else if(selectedObject == null || riverObjects.Contains(selectedObject))
                {
                    setupSelection();
                } 
                else 
                {
                    dropInventoryCheck();
                }
            }
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(waterSprite, destWater, sourceWater, Color.White);
            spriteBatch.Draw(waterSprite, destWaterAlt, sourceWater, Color.White);

            foreach (RiverObject obj in riverObjects)
            {
                obj.draw(spriteBatch);
            }

            spriteBatch.Draw(background, backgroundRect, Color.White);

            inventory.draw(spriteBatch);

            if (selectedObject != null && selectedObject.isSelected())
            {
                selectedObject.draw(spriteBatch);
            }

            spriteBatch.DrawString(font, statusString, new Vector2(2, 30), Color.White);
            bciInput.draw(spriteBatch);

            if (overlayWnd != null) overlayWnd.draw(spriteBatch);
            cursor.draw(spriteBatch);
        }

        public void debugSpeechInputGen()
        {
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
                statusString = "Voice command not found";
                appRef.insertLog(DataLog.DataType.Input, DataElement.DataType.VoiceRejected, "Voice command not found for specific window. (" + s + ")"); 
                return;
            }

            if (overlayWnd == null && resultID == 0)
            {
                overlayWnd = new ConfirmDialog(wndWidth, wndHeight, appRef); 
                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Opened Confirm Dialog");  
            }
            else if (overlayWnd != null && resultID == 1)
            {
                Rectangle selRect = getSelectionRect();
                if (selRect.X != 0)
                {
                    selectModeAction = SelectModeAction.Select;
                    cursor.beginSelection(500, selRect);
                }
            }
            else if (resultID == 2 || resultID == 3)
            {
                if((inputManager.getHandInputMode() != InputManager.HandInputMode.Hand_Right))
                {
                    inputManager.setHandInputMode(InputManager.HandInputMode.Hand_Right);
                    //statusString = "Input hand changed to right";
                    appRef.insertLog(DataLog.DataType.Input, DataElement.DataType.InputMisc, "Hands Swapped to Right");
                } else {
                    inputManager.setHandInputMode(InputManager.HandInputMode.Hand_Left);
                    //statusString = "Input hand changed to left";
                    appRef.insertLog(DataLog.DataType.Input, DataElement.DataType.InputMisc, "Hands Swapped to Left");
                }
            }
        }

        private void setupSelection()
        {
            if (selectedObject != null && !selectedObject.getBoundBox().Contains(cursor.getPoint()))
            {
                selectedObject = null;
                cursor.clearSelectionProgress();
                // Consider logging
                return;
            }

            /*Point modPoint = cursor.getPoint();
            modPoint.X -= 50;
            modPoint.Y -= 50;*/
            foreach (RiverObject obj in riverObjects)
            {
                if (obj.getBoundBox().Contains(cursor.getPoint()))
                {
                    selectedObject = obj;
                    //selectedObject.select();
                    selectModeAction = SelectModeAction.Select;
                    cursor.beginSelection(400, obj.getBoundBox());
                    // Consider logging
                }
            }
        }

        private void dropInventoryCheck()
        {
            if (inventory.isItemOverInventory(selectedObject))
            {
                selectModeAction = SelectModeAction.Select;
                string resultText = inventory.storeItem(selectedObject);
                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Item Dropped into Inventory: " + resultText);
                selectedObject = null;
                }
        }

        private Rectangle getSelectionRect()
        {
            if (overlayWnd != null)
            {
                if (overlayWnd.getWndType() == WndType.ConfirmDialog)
                {
                    return ((ConfirmDialog)overlayWnd).getSelectionRect(cursor.getPoint());
                }
                else
                {
                    return ((VictoryDialog)overlayWnd).getSelectionRect(cursor.getPoint());
                }
            }
            else
            {
                // DO obj get here (note: note used)
            }

            return new Rectangle(0, 0, 0, 0);
        }

        private void spawnUpdate(GameTime gameTime)
        {
            spawnCooldown -= gameTime.ElapsedGameTime.Milliseconds * (speed + 0.1f);
            if (spawnCooldown < 0)
            {
                int a = 0;
                int b = 1;
                int c = spawnIndexOrder[0];

                for (int i = 2; i < spawnIndexOrder.Length; i++)
                {
                    if (spawnIndexOrder[i] < spawnIndexOrder[a])
                    {
                        a = i;
                    }
                    else if (spawnIndexOrder[i] < spawnIndexOrder[b])
                    {
                        b = i;
                    }

                    if (spawnIndexOrder[i] > c)
                    {
                        c = spawnIndexOrder[i];
                    }
                }

                int spawnRow = (gen.NextDouble() < 0.5) ? a : b;
                RiverObject.ObjectType newType = (gen.NextDouble() < 0.7) ? RiverObject.ObjectType.Junk : RiverObject.ObjectType.Treasure;
                RiverObject newObj;
                Rectangle dest = new Rectangle(wndWidth + 50, 150 + spawnRow * 65, 50, 50);
                Vector2 offset = new Vector2(0, 150 + spawnRow * 65);

                if(newType == RiverObject.ObjectType.Junk)
                {
                    int imgID = (gen.NextDouble() < 0.5) ? 2 : 3;
                    newObj = new RiverObject(newType, offset, objSprites[imgID], objSprites[4], dest);
                    appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Item Spawned Junk offset: " + offset + " spawnrow: " + spawnRow);
                }
                else
                {
                    int imgID = (gen.NextDouble() < 0.5) ? 0 : 1;
                    newObj = new RiverObject(newType, offset, objSprites[imgID], objSprites[4], dest);
                    appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Item Spawned Treasure offset: " + offset + " spawnrow: " + spawnRow);
                }

                riverObjects.Add(newObj);
                spawnIndexOrder[spawnRow] = c + 1;

                spawnCooldown = gen.Next(800) + 400;
            }
        }

        public void updateWater(GameTime gameTime)
        {
            // Update water background
            waterX -= gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 100 * (speed + 0.6f);
            waterXAlt -= gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 100 * (speed + 0.6f);
            if (waterX + waterWidth < 0)
            {
                waterX = (int)waterXAlt + (int)waterWidth;
            }
            else if (waterXAlt + waterWidth < 0)
            {
                waterXAlt = (int)waterX + (int)waterWidth;
            }
            destWater.X = (int)waterX;
            destWaterAlt.X = (int)waterXAlt;
        }

        public void endLevel(int score, int junkCount, int treasureCount)
        {
            appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventPuzzleEnd, "Puzzle Successfully Completed");
            appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.SolutionData,
                             "Missed: " + missedTreasure + " Junk: " + junkCount + " Treasure: " + treasureCount
                             + " RealTime: " + inventory.getRealTime() + " ModTime: " + inventory.getModTime() );
            string missMessage = (missedTreasure > 0) ? " and missed " + missedTreasure : "";
            overlayWnd = new VictoryDialog("You have passed. You scored: " + score 
                                            + "\nYou collected " + treasureCount + " treasure"
                                            + missMessage + ".", 
                                            WndType.MainMenu, wndWidth, wndHeight, appRef);
        }
    }
}
