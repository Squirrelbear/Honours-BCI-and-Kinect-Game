using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class InfoWnd : WndHandle
    {
        public static string[] speechStrings = { "Next", "Ready" };

        private WndType nextWnd;
        private int curFrame;
        private int endFrame;
        private List<Texture2D> frames;
        private Rectangle frameDest;
        private Texture2D background;
        private Rectangle backgroundDest;
        private SpriteFont font;

        public InfoWnd(WndType nextWnd, List<Texture2D> frames, int wndWidth, int wndHeight, Game1 appRef)
            : base(WndType.InfoWnd, wndWidth, wndHeight, appRef)
        {
            this.frames = frames;
            this.nextWnd = nextWnd;
            curFrame = 0;
            endFrame = frames.Count-1;

            background = appRef.Content.Load<Texture2D>("InfoWnd\\background");
            backgroundDest = new Rectangle(0, 0, wndWidth, wndHeight);
            frameDest = new Rectangle(0, 0, wndWidth, 610);

            font = appRef.Content.Load<SpriteFont>("largeFont");
        }

        public override void update(GameTime gameTime)
        {
            if (appRef.getEnableDebugInput())
            {
                debugSpeechInputGen();
            }
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            /*Color c;
            int colourID = curFrame % 3;
            switch (colourID)
            {
                case 0:
                    c = Color.White;
                    break;
                case 1:
                    c = Color.Green;
                    break;
                default:
                    c = Color.Yellow;
                    break;
            }*/

            spriteBatch.Draw(background, backgroundDest, Color.White);
            if(curFrame < frames.Count) // just a backup error check
                spriteBatch.Draw(frames[curFrame], frameDest, Color.White);

            if (curFrame == endFrame)
            {
                spriteBatch.DrawString(font, "Say \"Ready\" to Start Puzzle",
                                            new Vector2(wndWidth - 400, wndHeight - 80),
                                            Color.Black);
            }
            else
            {
                spriteBatch.DrawString(font, "Say \"Next\" to Continue",
                                            new Vector2(wndWidth - 350, wndHeight - 80),
                                            Color.Black);
            }
        }

        public void debugSpeechInputGen()
        {
            if (inputManager.isKeyPressed(Microsoft.Xna.Framework.Input.Keys.D5)
                || inputManager.isBtnPressed(Microsoft.Xna.Framework.Input.Buttons.A))
            {
                // Next/Ready
                if(curFrame < endFrame)
                {
                    handleSpeechRecognised(speechStrings[0]);
                }
                else
                {
                    handleSpeechRecognised(speechStrings[1]);
                }
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
                if (curFrame == endFrame)
                    appRef.speakMessage("Please say ready if you are ready.");
                else
                    appRef.speakMessage("Please say next to continue.");
                return;
            }

            if (resultID == 1 && curFrame == endFrame)
            {
                appRef.setWnd(nextWnd);
            }
            else if (resultID == 0)
            {
                if (curFrame < endFrame)
                    curFrame++;
                else
                    appRef.speakMessage("Please say ready if you are ready.");
            }
        }
    }
}
