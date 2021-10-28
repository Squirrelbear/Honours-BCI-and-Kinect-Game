using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class MainMenu : WndHandle
    {
        // First exit game used to say "Start Puzzle"
        public static string[] speechStrings = { "Exit Game", "Exit Game", "change hands", "swap hands" };

        private Texture2D background;
        private Rectangle backgroundDest;
        private List<SpriteButton> buttons;
        private List<Texture2D> btnSpriteOns;
        private List<Texture2D> btnSpriteOffs;
        private Cursor cursor;
        private int curID;
        private int btnHeight, btnWidth;

        private Texture2D cursorSprite;
        private Texture2D cursorProgressSprite;

        private SpriteFont fontLarge;
        private SpriteFont fontRegular;

        public MainMenu(int wndWidth, int wndHeight, Game1 appRef)
            : base(WndType.MainMenu, wndWidth, wndHeight, appRef)
        {
            background = appRef.Content.Load<Texture2D>("MainMenu\\background");
            backgroundDest = new Rectangle(0, 0, wndWidth, wndHeight);
            
            cursorSprite = appRef.Content.Load<Texture2D>("TilePuzzle\\hand");
            cursorProgressSprite = appRef.Content.Load<Texture2D>("Common\\progress");
            cursor = new Cursor(cursorSprite, cursorProgressSprite);

            fontLarge = appRef.Content.Load<SpriteFont>("hugeFont");
            fontRegular = appRef.Content.Load<SpriteFont>("mediumFont");

            curID = -1;
            buttons = new List<SpriteButton>();
            btnSpriteOffs = new List<Texture2D>();
            btnSpriteOns = new List<Texture2D>();

            btnSpriteOffs.Add(appRef.Content.Load<Texture2D>("MainMenu\\storymodeout"));
            btnSpriteOffs.Add(appRef.Content.Load<Texture2D>("MainMenu\\tilepuzzleout"));
            btnSpriteOffs.Add(appRef.Content.Load<Texture2D>("MainMenu\\streetpuzzleout"));
            btnSpriteOffs.Add(appRef.Content.Load<Texture2D>("MainMenu\\riverpuzzleout"));
            btnSpriteOffs.Add(appRef.Content.Load<Texture2D>("MainMenu\\exitgameout"));
            btnSpriteOffs.Add(appRef.Content.Load<Texture2D>("MainMenu\\calibrationout"));

            btnSpriteOns.Add(appRef.Content.Load<Texture2D>("MainMenu\\storymodeover"));
            btnSpriteOns.Add(appRef.Content.Load<Texture2D>("MainMenu\\tilepuzzleover"));
            btnSpriteOns.Add(appRef.Content.Load<Texture2D>("MainMenu\\streetpuzzleover"));
            btnSpriteOns.Add(appRef.Content.Load<Texture2D>("MainMenu\\riverpuzzleover"));
            btnSpriteOns.Add(appRef.Content.Load<Texture2D>("MainMenu\\exitgameover"));
            btnSpriteOns.Add(appRef.Content.Load<Texture2D>("MainMenu\\calibrationover"));

            btnWidth = 256;
            btnHeight = 80;

            buttons.Add(new SpriteButton(new Rectangle(wndWidth / 4 - btnWidth / 2, wndHeight / 2 - (int)(btnHeight * 2.5f) - 30, btnWidth, btnHeight),
                                           btnSpriteOns[5], btnSpriteOffs[5], (int)WndType.Calibration));
            buttons.Add(new SpriteButton(new Rectangle(wndWidth / 4 - btnWidth / 2, wndHeight / 2 - (int)(btnHeight * 1.5f) - 20, btnWidth, btnHeight),
                                           btnSpriteOns[0], btnSpriteOffs[0], (int)WndType.TilePuzzle));
            buttons.Add(new SpriteButton(new Rectangle(wndWidth / 4 - btnWidth / 2, wndHeight / 2 - (int)(btnHeight * 0.5f) - 10, btnWidth, btnHeight),
                                           btnSpriteOns[1], btnSpriteOffs[1], (int)WndType.TilePuzzle));
            buttons.Add(new SpriteButton(new Rectangle(wndWidth / 4 - btnWidth / 2, wndHeight / 2 + (int)(btnHeight * 0.5f), btnWidth, btnHeight),
                                           btnSpriteOns[2], btnSpriteOffs[2], (int)WndType.StreetPuzzle));
            buttons.Add(new SpriteButton(new Rectangle(wndWidth / 4 - btnWidth / 2, wndHeight / 2 + (int)(btnHeight * 1.5f) + 10, btnWidth, btnHeight),
                                           btnSpriteOns[3], btnSpriteOffs[3], (int)WndType.RiverPuzzle));
            buttons.Add(new SpriteButton(new Rectangle(wndWidth / 4 - btnWidth / 2, wndHeight / 2 + (int)(btnHeight * 2.5f) + 20, btnWidth, btnHeight),
                                           btnSpriteOns[4], btnSpriteOffs[4], (int)WndType.Quit));

            // block formation
            /*buttons.Add(new SpriteButton(new Rectangle(wndWidth / 2 - btnWidth / 2, wndHeight / 2 - 256 - btnHeight / 2, 128, 128), 
                                           btnSpriteOns[0], btnSpriteOffs[0], (int)WndType.TilePuzzle));
            buttons.Add(new SpriteButton(new Rectangle(wndWidth / 2 + 256 - btnWidth / 2, wndHeight / 2 - btnHeight / 2, 128, 128), 
                                           btnSpriteOns[1], btnSpriteOffs[1], (int)WndType.RiverPuzzle));
            buttons.Add(new SpriteButton(new Rectangle(wndWidth / 2 - btnWidth / 2, wndHeight / 2 + 256 - btnHeight / 2, 128, 128), 
                                           btnSpriteOns[2], btnSpriteOffs[2], (int)WndType.StreetPuzzle));
            buttons.Add(new SpriteButton(new Rectangle(wndWidth / 2 - 256 - btnWidth / 2, wndHeight / 2 - btnHeight / 2, 128, 128), 
                                           btnSpriteOns[3], btnSpriteOffs[3], (int)WndType.TilePuzzle));*/
        }

        public override void update(GameTime gameTime)
        {
            if (inputManager.getHandInputMode() == InputManager.HandInputMode.Hand_Right)
            {
                cursor.setPoint(inputManager.getHandPosition(true, true));
            }
            else
            {
                cursor.setPoint(inputManager.getHandPosition(false, true));
            }
            cursor.update(gameTime);
            curID = -1;
            foreach (SpriteButton b in buttons)
            {
                if (b.isPointInButton(cursor.getPoint()))
                {
                    b.setOver(true);
                    curID = b.getActionID();

                    if (!cursor.isSelecting() && b.getActionID() != (int)WndType.Quit)
                    {
                        cursor.beginSelection(1500, b.getRect());
                    }
                }
                else
                {
                    b.setOver(false);
                }
            }

            if (cursor.isSelectionComplete())
            {
                WndType type = (WndType)curID;

                cursor.clearSelectionProgress();
                //appRef.setWnd(type);
                if (type == WndType.Quit || type == WndType.Calibration)
                {
                    appRef.setWnd(type);
                }
                else
                {
                    appRef.setInfoWnd(type);
                }
            }

            if (appRef.getEnableDebugInput())
            {
                debugSpeechInputGen();
            }
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, backgroundDest, Color.White);
            foreach (SpriteButton b in buttons)
            {
                b.draw(spriteBatch);
            }

            if (curID == 5)
            {
                spriteBatch.DrawString(fontLarge, "Say \"Exit Game\"",
                                        new Vector2(wndWidth / 4 - 120, wndHeight / 2 - btnHeight * 2.5f - 64),
                                        Color.Red);
                spriteBatch.DrawString(fontLarge, "Say \"Exit Game\"",
                                        new Vector2(wndWidth / 4 - 120, wndHeight / 2 + btnHeight * 2.5f + 110),
                                        Color.Red);
            }
            /*else if (curID != -1)
            {
                spriteBatch.DrawString(fontLarge, "Say \"Start Puzzle\"",
                                        new Vector2(wndWidth / 4 - 135, wndHeight / 2 - btnHeight * 2 - 64),
                                        Color.Red);
                spriteBatch.DrawString(fontLarge, "Say \"Start Puzzle\"",
                                        new Vector2(wndWidth / 4 - 135, wndHeight / 2 + btnHeight * 2 + 110),
                                        Color.Red);
            }*/

            spriteBatch.DrawString(fontRegular, "Developed by Peter Mitchell (2012)",
                                        new Vector2(wndWidth - 350, wndHeight - 60),
                                        Color.White);

            appRef.drawKinectDepthImage();

            cursor.draw(spriteBatch);
        }

        public void debugSpeechInputGen()
        {
            if (inputManager.isKeyPressed(Microsoft.Xna.Framework.Input.Keys.D5)
                || inputManager.isBtnPressed(Microsoft.Xna.Framework.Input.Buttons.A))
            {
                // Start Puzzle
                handleSpeechRecognised(speechStrings[0]);
            } else if(inputManager.isKeyPressed(Microsoft.Xna.Framework.Input.Keys.D4)
                || inputManager.isBtnPressed(Microsoft.Xna.Framework.Input.Buttons.B))
            {
                // Exit Game
                handleSpeechRecognised(speechStrings[0]);
            }
            else if (inputManager.isKeyPressed(Microsoft.Xna.Framework.Input.Keys.D3))
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
                return;
            }

            if (/*(resultID == 0 && curID != 5) ||*/ (resultID == 0 && curID == 5))
            {
                // Start Puzzle || Exit Game (code 5)
                Rectangle selRect = new Rectangle(0, 0, 0, 0);
                foreach (SpriteButton b in buttons)
                {
                    if (b.isPointInButton(cursor.getPoint()))
                    {
                        selRect = b.getRect();
                        break;
                    }
                }

                cursor.beginSelection(500, selRect);
            }
            else if(resultID == 2 || resultID == 3)
            {
                if ((inputManager.getHandInputMode() != InputManager.HandInputMode.Hand_Right))
                {
                    inputManager.setHandInputMode(InputManager.HandInputMode.Hand_Right);
                    //statusString = "Input hand changed to right";
                    appRef.insertLog(DataLog.DataType.Input, DataElement.DataType.InputMisc, "Hands Swapped to Right");
                }
                else
                {
                    inputManager.setHandInputMode(InputManager.HandInputMode.Hand_Left);
                    //statusString = "Input hand changed to left";
                    appRef.insertLog(DataLog.DataType.Input, DataElement.DataType.InputMisc, "Hands Swapped to Left");
                }
            }
        }
    }
}
