using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace HonoursGame
{
    public class TilePuzzle : WndHandle
    {
        public static string[] speechStrings = { "rotate tile left",
                                             "rotate tile right",
                                             "drop tile", "place tile",
                                             "change hands", "swap hands",
                                             "select tile", "grab tile", 
                                              "pause game", 
                                              "confirm"};

        public enum SelectModeAction { None, Drop, Select };

        private int paddingX, paddingY;
        private int modPaddingX, modPaddingY;
        
        private int tileSize;
        private int multiplierFactor;

        private SelectModeAction selectModeAction;
        private Tile[] tiles;
        private Tile selectedTile;
        private Cursor cursor;

        private Texture2D[] baseTileSprites;
        private bool[] baseSpriteAllocated;
        private Texture2D cursorSprite;
        private Texture2D cursorProgressSprite;
        private Texture2D[] solTileSprites;
        private Texture2D emptyTileSprite;
        private Texture2D background;
        private Rectangle backgroundRect;
        //private float thresholdCooldown;
        private float THRESHOLD;
        private float maxAlphaMod;
        private float maxAlphaCooldown; 

        private Texture2D glowTexture;
        private Rectangle glowRect;
        private float glowValue;
        private float glowTarget;
        private bool glowIncrease;

        private Random gen;
        
        private string statusString;
        private SpriteFont font;
        private SpriteFont fontLarge;

        private WndHandle overlayWnd;

        private float secondsTaken;
        private int movesTaken;
        private int minMoves;

        private ScaledBCI bciInput;

        public TilePuzzle(int wndWidth, int wndHeight, Game1 appRef)
            : base(WndType.TilePuzzle, wndWidth, wndHeight, appRef)
        {
            appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventPuzzleStart, "Tile Puzzle Started");
            gen = new Random();

            // New ScaledInput using the Alpha mean
            // < 0.05 (concentrating) -> decrease by 0.05
            // > 0.2 (relaxing) -> increase by 0.1
            // Scale -> min = 0, max = 0.6
            //bciInput = new ScaledBCI(inputManager, appRef, -1, -1, 0, 0, 0.6f, -0.05f, -0.1f, 0.2f, 0.05f);

            //bciInput.configThresholdMod(-0.075f, 0.075f, 0.14f, 0.9f, 500, 500);

            //bciInput = new ScaledBCI(inputManager, appRef, -1, -1, 0, 0, 0.6f, -0.05f, -0.1f, 0.01f, 0.005f);
            bciInput = new ScaledBCI(inputManager, appRef, -1, -1, 0, 0, 0.6f, 0.05f, 0.1f, 0.4f, 0.35f);
            //bciInput.configThresholdMod(-0.075f, 0.075f, 0.14f, 0.9f, 500, 500);

            Rectangle scaledBCIDest = new Rectangle(15, (wndHeight - 200) / 2, 75, 200);
            Rectangle scaledBCIProgressDest = new Rectangle(scaledBCIDest.X + (75 - 37) / 2 - 1, scaledBCIDest.Y + (200 - 159) / 2, 37, 159);
            bciInput.configGraphics(scaledBCIDest, scaledBCIProgressDest, false);

            baseTileSprites = new Texture2D[9];
            solTileSprites = new Texture2D[9];
            baseSpriteAllocated = new bool[9];
            for (int i = 0; i < 9; i++)
            {
                baseTileSprites[i] = appRef.Content.Load<Texture2D>("TilePuzzle\\t" + (i + 1));
                solTileSprites[i] = appRef.Content.Load<Texture2D>("TilePuzzle\\s" + (i + 1));
                baseSpriteAllocated[i] = false;
            }
            emptyTileSprite = appRef.Content.Load<Texture2D>("TilePuzzle\\emptytile");
            background = appRef.Content.Load<Texture2D>("TilePuzzle\\tilebackground");
            backgroundRect = new Rectangle(0, 0, background.Width, background.Height);

            cursorSprite = appRef.Content.Load<Texture2D>("TilePuzzle\\hand");
            cursorProgressSprite = appRef.Content.Load<Texture2D>("Common\\progress");
            font = appRef.Content.Load<SpriteFont>("smallFont");
            fontLarge = appRef.Content.Load<SpriteFont>("hugeFont");
            statusString = "";

            paddingX = 50;
            paddingY = 50;
            this.wndHeight = wndHeight;
            this.wndWidth = wndWidth;

            if (wndWidth > wndHeight)
            {
                modPaddingY = paddingY;
                multiplierFactor = (wndHeight - 2 * paddingY) / 3;
                tileSize = multiplierFactor;//(int)(multiplierFactor * 0.9);
                modPaddingX = (wndWidth - multiplierFactor * 3) / 2;
            }
            else
            {
                modPaddingX = paddingX;
                multiplierFactor = (wndWidth - 2 * paddingX) / 3;
                tileSize = multiplierFactor;//(int)(multiplierFactor * 0.9);
                modPaddingY = (wndHeight - multiplierFactor * 3) / 2;
            }

            glowTexture = appRef.Content.Load<Texture2D>("TilePuzzle\\glow");
            glowRect = new Rectangle(modPaddingX - 30, modPaddingY - 30, wndWidth - 2*modPaddingX + 60, wndHeight - 2*modPaddingY + 60);
            glowValue = -1;

            tiles = new Tile[9];
            ArrayList unAssignedRands = new ArrayList();
            for (int i = 1; i <= 9; i++)
            {
                unAssignedRands.Add(i);
            }

            //Console.WriteLine("Solution rotations: ");
            minMoves = 0;
            for (int i = 1; i <= 9; i++)
            {
                int solIndex = gen.Next(0, unAssignedRands.Count);
                int initRotation = gen.Next(0, 4);
                //Console.WriteLine((int)unAssignedRands[solIndex] + ": " + solRotation);

                if (i != (int)unAssignedRands[solIndex])
                {
                    minMoves += 2; // select; ignoring drop by using the shortcut method
                }
                else
                {
                    if (initRotation != 0)
                    {
                        minMoves += 2;
                    }
                }

                if (initRotation == 1 || initRotation == 3)
                {
                    minMoves++; // one rotation left or right
                }
                else if (initRotation == 2)
                {
                    minMoves += 2; // two rotations left or right
                }

                tiles[i - 1] = new Tile(i, tileSize, tileSize, (int)unAssignedRands[solIndex], initRotation, this);
                appRef.insertLog(DataLog.DataType.Misc, DataElement.DataType.PuzzleData, "Tile " + i + ": " + (int)unAssignedRands[solIndex] + " " + initRotation);
                unAssignedRands.RemoveAt(solIndex);
            }
            Console.WriteLine("Minimum moves is: " + minMoves);
            appRef.insertLog(DataLog.DataType.Misc, DataElement.DataType.Misc, "Minimum moves is: " + minMoves);

            cursor = new Cursor(cursorSprite, cursorProgressSprite);

            selectedTile = null;

            /*Console.WriteLine("Grid points: ");
            for (int i = 1; i <= 9; i++)
            {
                Point tmp = getGridPointbyRef(i);
                Console.WriteLine(tmp.X + ", " + tmp.Y);
            }*/
            overlayWnd = null;
            secondsTaken = 0;

            THRESHOLD = 0.2f;
            //thresholdCooldown = 1000 * 45; // 45 seconds
            maxAlphaCooldown = 0;
            maxAlphaMod = 0;
        }

        public override void update(GameTime gameTime)
        {
            updateCursor(gameTime);

            if (overlayWnd == null)
            {

                /*thresholdCooldown -= gameTime.ElapsedGameTime.Milliseconds;
                if (thresholdCooldown < 0)
                {
                    THRESHOLD -= THRESHOLD / 15;
                    appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Threshold Change: " + THRESHOLD);
                    thresholdCooldown = 1000 * 30; // 30 seconds
                }

                float fadeValue = getModifiedAlpha(inputManager.getAlphaState(), inputManager.getOldAlphaState(), gameTime);
                if (fadeValue < 0) fadeValue = 0;
                if (fadeValue > 0.6f) fadeValue = 0.6f;*/
                bciInput.update(gameTime);
                float fadeValue = bciInput.getScaledValue();
                if (selectedTile != null) selectedTile.setLocation(cursor.getPoint());
                foreach (Tile tile in tiles)
                {
                    tile.setFadeValue(fadeValue);
                    tile.update(gameTime);
                }
            }

            if (glowValue != -1)
            {
                if (glowIncrease && glowValue >= glowTarget)
                {
                    glowTarget = gen.Next(0, 55);
                    glowIncrease = false;
                }
                else if (!glowIncrease && glowValue <= glowTarget)
                {
                    glowTarget = gen.Next(200, 255);
                    glowIncrease = true;
                }
                else if (glowIncrease)
                {
                    glowValue += 0.1f * gameTime.ElapsedGameTime.Milliseconds;
                }
                else
                {
                    glowValue -= 0.1f * gameTime.ElapsedGameTime.Milliseconds;
                }
            }

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

            if (overlayWnd == null)
            {
                secondsTaken += gameTime.ElapsedGameTime.Milliseconds;
            }
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
                        beginSelectTile();
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
                                overlayWnd = null;
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
                                appRef.setInfoWnd(WndType.StreetPuzzle);
                            }
                        }
                    }
                }
                else if (selectModeAction == SelectModeAction.Drop)
                {
                    dropTile();
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
            }
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, backgroundRect, Color.White);
            if(glowValue != -1)
                spriteBatch.Draw(glowTexture, glowRect, Color.White * (glowValue/255.0f));
            foreach (Tile tile in tiles)
            {
                if (tile == selectedTile) continue;
                tile.draw(spriteBatch);
            }
            if (selectedTile != null) selectedTile.draw(spriteBatch);
            spriteBatch.DrawString(font, statusString, new Vector2(2, 30), Color.White);
            spriteBatch.DrawString(fontLarge, (secondsTaken / 1000.0f).ToString("0.0"), new Vector2(wndWidth - 100, wndHeight - 50), Color.Red);
            //spriteBatch.DrawString(font, "Threshold: " + THRESHOLD + " Next: " + (thresholdCooldown / 1000), new Vector2(10, 90), Color.White);
            bciInput.draw(spriteBatch);
            if (overlayWnd != null) overlayWnd.draw(spriteBatch);
            cursor.draw(spriteBatch);
        }

        public Point getGridPointbyRef(int gridRef)
        {
            int y = (gridRef -1) / 3;
            int x = (gridRef -1) % 3;
            return new Point(modPaddingX + x * multiplierFactor, modPaddingY + y * multiplierFactor);
        }

        private bool isSolved()
        {
            foreach (Tile tile in tiles)
            {
                if (!tile.isSolution()) return false;
            }

            return true; 
        }

        public void beginSelectTile()
        {
            if (selectedTile != null)
            {
                appRef.speakMessage("You already have a tile!");
                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Tile selection halted. Already holding tile.");
                return;
            }

            foreach (Tile tile in tiles)
            {
                if (tile.isPointInThis(cursor.getPoint()))
                {
                    selectedTile = tile;
                    statusString = "Tile successfully selected";
                    appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Tile selected. ID: " + selectedTile.getGridRef());
                    selectedTile.setSelected(true);
                    movesTaken++;
                    return;
                }
            }

            statusString = "Tile not selected";
            appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Tile selection halted. Cursor was not over tile.");
        }

        public void rotateTile(bool rotateRight)
        {
            if (selectedTile != null)
            {
                float amount = selectedTile.rotate(rotateRight);
                statusString = "Tile successfully rotated " + ((rotateRight) ? "Right " : "Left ") + amount;
                movesTaken++;
                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, statusString);
            }
            else
            {
                statusString = "No tile selected.";
                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Tile rotation halted. No tile selected.");
            }
        }

        public void dropTile()
        {
            if (selectedTile == null)
            {
                appRef.speakMessage("You don't have a tile selected.");
                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Tile drop halted. No tile selected.");
                return;
            } 

            Tile replaceLocation = null;
            foreach (Tile tile in tiles)
            {
                if (tile.isPointInThis(cursor.getPoint()))
                {
                    replaceLocation = tile;
                    break;
                }
            }

            if (replaceLocation == null)
            {
                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Tile drop halted. No valid drop location.");
                return;
            }

            if (replaceLocation == selectedTile)
            {
                // place the current tile back in it's current square
                selectedTile.setSelected(false);
                selectedTile.setGridRef(selectedTile.getGridRef());
                statusString = "Tile successfully placed down (" + selectedTile.isSolution() + ")";
                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc,
                                    "Tile dropped on own square. ID: " + selectedTile.getGridRef()
                                    + " Correct: " + selectedTile.isSolution().ToString());
                selectedTile = null;
                movesTaken++;

                // test for solution
                if (isSolved())
                {
                    overlayWnd = new VictoryDialog("Success! You completed the tile puzzle in\n" + 
                                                    secondsTaken/1000 + " seconds.",
                                                    WndType.StreetPuzzle, wndWidth, wndHeight, appRef);
                    //Console.WriteLine("PUZZLE COMLETE");
                    statusString = "PUZZLE COMPLETE";
                    appRef.speakMessage("Puzzle Complete.");
                    appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventPuzzleEnd, "Puzzle Complete");
                    appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventPuzzleEnd, "Puzzle Stats. Min Moves: " + minMoves + " Moves taken: " + movesTaken + " In Time: " + secondsTaken / 1000);
                    //Console.WriteLine("Min Moves: " + minMoves + " Moves taken: " + movesTaken + " In Time: " + secondsTaken / 1000);
                    glowValue = 0;
                    glowTarget = 255;
                }
                return;
            }

            int tempGridRef = selectedTile.getGridRef();
            statusString = "Tile swapped with down one (" + selectedTile.isSolution() + ")";
            selectedTile.setSelected(false);
            selectedTile.setGridRef(replaceLocation.getGridRef());
            int newGridRef = replaceLocation.getGridRef();
            replaceLocation.setGridRef(tempGridRef);
            selectedTile = replaceLocation;
            selectedTile.setSelected(true);
            movesTaken+=2;
            appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Tile swap performed. ID " + tempGridRef + " with " + newGridRef);

            // determine if Tile is somewhere that can be dropped and which place that is
            // drop and initiate the transition of the below tile to the new location

        }

        public void debugSpeechInputGen()
        {
            if (inputManager.isKeyPressed(Microsoft.Xna.Framework.Input.Keys.D1)
                || inputManager.isBtnPressed(Microsoft.Xna.Framework.Input.Buttons.DPadLeft))
            {
                // rotate tile left
                handleSpeechRecognised(speechStrings[0]);
            }
            else if (inputManager.isKeyPressed(Microsoft.Xna.Framework.Input.Keys.D2)
                || inputManager.isBtnPressed(Microsoft.Xna.Framework.Input.Buttons.DPadRight))
            {
                // rotate tile right
                handleSpeechRecognised(speechStrings[1]);
            }
            else if (inputManager.isKeyPressed(Microsoft.Xna.Framework.Input.Keys.D3)
                || inputManager.isBtnPressed(Microsoft.Xna.Framework.Input.Buttons.B))
            {
                // drop tile
                handleSpeechRecognised(speechStrings[2]);
            }
            else if (inputManager.isKeyPressed(Microsoft.Xna.Framework.Input.Keys.D4))
            {
                // swap hands
                handleSpeechRecognised(speechStrings[4]);
            }
            else if (inputManager.isKeyPressed(Microsoft.Xna.Framework.Input.Keys.D5)
                || inputManager.isBtnPressed(Microsoft.Xna.Framework.Input.Buttons.A))
            {

                if (overlayWnd == null)
                {
                    // select tile
                    handleSpeechRecognised(speechStrings[6]);
                }
                else
                {
                    // select button
                    handleSpeechRecognised(speechStrings[9]);
                }
            }
            else if (inputManager.isKeyPressed(Microsoft.Xna.Framework.Input.Keys.D6)
                || inputManager.isBtnPressed(Microsoft.Xna.Framework.Input.Buttons.Start))
            {
                // main menu
                handleSpeechRecognised(speechStrings[8]);
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
                handleSpeechNotRecognised();
                appRef.insertLog(DataLog.DataType.Input, DataElement.DataType.VoiceRejected, "Voice command not found for specific window. ("+s+")"); 
                return;
            }

            if (resultID < 8 && overlayWnd != null)
            {
                // stops attempt to use a tile function while in confirmation dialog
                statusString = "ERROR: You can't do that at the moment (" + s + ")";
                appRef.speakMessage("You can't do that at the moment.");
                Console.WriteLine("ERROR: You can't do that at the moment (" + s + ")");
                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Invalid command used while overlay window up. (" + s + ")");
                return;
            }
            else if (resultID < 4 && selectedTile == null)
            {
                statusString = "ERROR: Must have a tile selected to do that (" + s + ")";
                appRef.speakMessage("You need to select a tile to do that.");
                Console.WriteLine("ERROR: Must have a tile selected to do that (" + s + ")");
                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Tile rotation or drop halted. Not holding a tile. (" + s + ")");
                return;
            }
            else if ((resultID == 6 || resultID == 7) && selectedTile != null)
            {
                statusString = "ERROR: You already have a tile selected (" + s + ")";
                appRef.speakMessage("You already have a tile selected.");
                Console.WriteLine("ERROR: You already have a tile selected");

                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Tile select halted. Already holding a tile. (" + s + ")");
                return;
            }
            else if ((resultID == 8 && overlayWnd != null) || (resultID > 8 && overlayWnd == null))
            {
                statusString = "ERROR: overlay already open or no overlay open (" + s + ")";
                appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Command cancelled. Overlay already open or no overlay open. (" + s + ")");
                return;
            }

            switch (resultID)
            {
                case 0:
                    rotateTile(false);
                    break;
                case 1: 
                    rotateTile (true);
                    break;
                case 2: 
                case 3:
                    //dropTile();
                    Rectangle dropRect = getSelectionRect();
                    if (dropRect.X != 0)
                    {
                        selectModeAction = SelectModeAction.Drop;
                        cursor.beginSelection(500, dropRect);
                    }
                    break;
                case 4:
                case 5:
                    if((inputManager.getHandInputMode() != InputManager.HandInputMode.Hand_Right))
                    {
                        inputManager.setHandInputMode(InputManager.HandInputMode.Hand_Right);
                        statusString = "Input hand changed to right";
                        appRef.insertLog(DataLog.DataType.Input, DataElement.DataType.InputMisc, "Hands Swapped to Right");
                    } else {
                        inputManager.setHandInputMode(InputManager.HandInputMode.Hand_Left);
                        statusString = "Input hand changed to left";
                        appRef.insertLog(DataLog.DataType.Input, DataElement.DataType.InputMisc, "Hands Swapped to Left");
                    }
                    break;
                case 6:
                case 7: 
                case 9:
                    //beginSelectTile();
                    Rectangle selRect = getSelectionRect();
                    if (selRect.X != 0)
                    {
                        selectModeAction = SelectModeAction.Select;
                        cursor.beginSelection(500, selRect);
                    }
                    break;
                case 8:
                    if (overlayWnd == null)
                    {
                        appRef.insertLog(DataLog.DataType.Event, DataElement.DataType.EventMisc, "Opened Confirm Dialog");
                        overlayWnd = new ConfirmDialog(wndWidth, wndHeight, appRef);
                    }
                    break;
            }
        }

        public Texture2D getSolTexture(int id)
        {
            return solTileSprites[id-1];
        }

        public Texture2D getBaseTexture(int id)
        {
            return baseTileSprites[id-1];
        }

        public Texture2D getEmptyTexture()
        {
            return emptyTileSprite;
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
                foreach (Tile tile in tiles)
                {
                    if (tile.isPointInThis(cursor.getPoint()))
                    {
                        return tile.getBoundBox();
                    }
                }
            }

            return new Rectangle(0, 0, 0, 0);
        }

        public override void handleSpeechNotRecognised()
        {
            appRef.speakMessage("Sorry. I did not understand what you said.");
            appRef.insertLog(DataLog.DataType.Input, DataElement.DataType.VoiceMisc, "Speech not recognised.");
        }

        public float getThreshold()
        {
            return THRESHOLD;
        }

        private float getModifiedAlpha(float currentAlpha, float oldalpha, GameTime gameTime)
        {
            // perform an update first
            if (maxAlphaMod > 0)
            {
                maxAlphaCooldown -= gameTime.ElapsedGameTime.Milliseconds;
                if (maxAlphaCooldown <= -1)
                {
                    // work around to sudden drops
                    inputManager.setOldAlphaState(maxAlphaMod);
                    maxAlphaMod = 0;
                }
                else if (maxAlphaCooldown > 0)
                {
                    if (currentAlpha >= maxAlphaMod)
                    {
                        maxAlphaMod = currentAlpha;
                        maxAlphaCooldown = 5000;
                    }
                    else
                    {
                        maxAlphaMod -= 0.1f * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                    }
                }
                else
                {
                    currentAlpha = inputManager.applySmoothing(currentAlpha, maxAlphaMod);
                }
            }
            else if (currentAlpha > 0.4)
            {
                maxAlphaMod = currentAlpha;
                maxAlphaCooldown = 5000;
            }
            else
            {
                currentAlpha = inputManager.applySmoothing(currentAlpha, oldalpha);
            }

            // then return the current value
            if (maxAlphaMod > 0 && maxAlphaCooldown > 0)
            {
                return maxAlphaMod;
            }
            else
            {
                return currentAlpha;
            }
        }
    }
}
