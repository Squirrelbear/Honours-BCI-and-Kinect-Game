using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    class VictoryDialog : WndHandle
    {
        public static string[] speechStrings = { };

        private Texture2D background;
        private Rectangle backgroundDest;
        private List<SpriteButton> buttons;
        private List<Texture2D> btnSpriteOns;
        private List<Texture2D> btnSpriteOffs;
        private int curID;
        private int btnHeight, btnWidth;
        private Point cursorPos;

        private SpriteFont fontLarge;
        private String message;

        public VictoryDialog(String message, WndHandle.WndType nextWnd, int wndWidth, int wndHeight, Game1 appRef)
            : base(WndType.VictoryDialog, wndWidth, wndHeight, appRef)
        {
            background = appRef.Content.Load<Texture2D>("MainMenu\\background");
            backgroundDest = new Rectangle(wndWidth / 2 - 300, wndHeight / 2 - 150, 600, 300);

            fontLarge = appRef.Content.Load<SpriteFont>("largeFont");
            this.message = message;

            curID = -1;
            cursorPos = new Point(0, 0);
            buttons = new List<SpriteButton>();
            btnSpriteOffs = new List<Texture2D>();
            btnSpriteOns = new List<Texture2D>();

            btnSpriteOffs.Add(appRef.Content.Load<Texture2D>("OtherDialog\\mainmenuout"));
            btnSpriteOffs.Add(appRef.Content.Load<Texture2D>("OtherDialog\\nextpuzzleout"));

            btnSpriteOns.Add(appRef.Content.Load<Texture2D>("OtherDialog\\mainmenuover"));
            btnSpriteOns.Add(appRef.Content.Load<Texture2D>("OtherDialog\\nextpuzzleover"));

            btnWidth = 256;
            btnHeight = 80;

            buttons.Add(new SpriteButton(new Rectangle(wndWidth / 2 - btnWidth - 20, wndHeight / 2 + 50, btnWidth, btnHeight),
                                           btnSpriteOns[0], btnSpriteOffs[0], (int)WndType.MainMenu));
            buttons.Add(new SpriteButton(new Rectangle(wndWidth / 2 + 20, wndHeight / 2 + 50, btnWidth, btnHeight),
                                           btnSpriteOns[1], btnSpriteOffs[1], (int)nextWnd));
        }

        public override void update(GameTime gameTime)
        {
            foreach (SpriteButton b in buttons)
            {
                if (b.isPointInButton(cursorPos))
                {
                    b.setOver(true);
                    curID = b.getActionID();
                }
                else
                {
                    b.setOver(false);
                }
            }
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, backgroundDest, Color.White);

            foreach (SpriteButton b in buttons)
            {
                b.draw(spriteBatch);
            }

            spriteBatch.DrawString(fontLarge, message,
                                        new Vector2(wndWidth / 2 - btnWidth - 20, wndHeight / 2 - 70),
                                        Color.Black);

           // spriteBatch.DrawString(fontLarge, "All progress will be lost on current puzzle!",
            //                            new Vector2(wndWidth / 2 - btnWidth - 20, wndHeight / 2 - 40),
            //                            Color.Black);

            if (curID != -1)
            {
                spriteBatch.DrawString(fontLarge, "Say \"Confirm\"",
                                        new Vector2(wndWidth / 2 - 80, wndHeight / 2),
                                        Color.Red);
            }
        }

        public void updateCursor(Point cursorPos)
        {
            this.cursorPos = cursorPos;
        }

        public WndHandle.WndType getAction()
        {
            return (WndHandle.WndType)curID;
        }

        public void resetSelection()
        {
            curID = -1;
        }

        public Rectangle getSelectionRect(Point cursorPos)
        {
            curID = -1;
            foreach (SpriteButton b in buttons)
            {
                curID++;
                if (b.isPointInButton(cursorPos))
                {
                    return b.getRect();
                }
            }

            curID = -1;
            return new Rectangle(0, 0, 0, 0);
        }
    }
}
