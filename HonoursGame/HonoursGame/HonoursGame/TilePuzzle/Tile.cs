using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class Tile
    {
        // Location and Dimenions Variables
        // curPoint = top left corner of current dragged position
        // gridPoint = top left corner of grid location
        private Point curPoint, gridPoint;
        private Vector2 origin;
        private int width, height;
        private Rectangle dest, source, selectionBox;
        // the current grid location by index { {1,2,3}, {4,5,6}, {7,8,9} }
        private int gridRef;

        // State and solution variables
        private float rotation;
        private int simpleRotation;
        private int baseRotation;
        private float baseActualRotation;
        private float fadeValue;
        private bool selected;
        //private bool translating;
        private int solGridRef;

        // Graphics variables:
        protected Texture2D spriteBase;
        protected Texture2D spriteOverlay;
        protected Texture2D spriteEmptyCell;

        // Application variables
        private TilePuzzle tilePuzzle;

        private float THRESHOLD;

        public Tile(int gridRef, int width, int height, int solGridRef, int initRotaton, TilePuzzle tilePuzzle)
        {
            this.tilePuzzle = tilePuzzle;
            this.gridRef = gridRef;
            this.width = width;
            this.height = height;
            this.solGridRef = solGridRef;

            simpleRotation = initRotaton;
            rotation = simpleRotation * MathHelper.PiOver2;
            baseRotation = 0;
            baseActualRotation = 0;
            fadeValue = 0;
            selected = false;
            //translating = false;
            THRESHOLD = tilePuzzle.getThreshold();

            // Load textures
            spriteBase = tilePuzzle.getBaseTexture(gridRef);
            spriteOverlay = tilePuzzle.getSolTexture(solGridRef);
            spriteEmptyCell = tilePuzzle.getEmptyTexture();

            gridPoint = tilePuzzle.getGridPointbyRef(gridRef);
            selectionBox = new Rectangle(gridPoint.X, gridPoint.Y, width, height);
            curPoint = tilePuzzle.getGridPointbyRef(gridRef);
            dest = new Rectangle(curPoint.X+width/2, curPoint.Y+height/2, width, height);
            source = new Rectangle(0, 0, spriteBase.Width, spriteBase.Height);
            origin = new Vector2(width/2+20,height/2+20);//width / 2, height / 2);
        }

        public void update(GameTime gameTime)
        {
            THRESHOLD = tilePuzzle.getThreshold();
            /*if (translating)
            {
             * 
            }*/
        }

        public void draw(SpriteBatch spriteBatch)
        {
            // render the empty cell in the current cell
            spriteBatch.Draw(spriteEmptyCell, new Rectangle(gridPoint.X, gridPoint.Y, width, height), Color.White);
            spriteBatch.Draw(spriteBase, dest, source, Color.White, baseActualRotation, origin, SpriteEffects.None, 0f);
            if (fadeValue > THRESHOLD)
            {
               // tilePuzzle.getAppRef().enableAdditiveBlend(true);
                spriteBatch.Draw(spriteOverlay, dest, source, Color.White * ((fadeValue - THRESHOLD) / 0.7f), rotation, origin, SpriteEffects.None, 0f);
              //  tilePuzzle.getAppRef().enableAdditiveBlend(false);
            }
               // spriteBatch.Draw(spriteOverlay, dest, new Color(255,255,255, (fadeValue-THRESHOLD)/0.8f*150));
        }

        public float rotate(bool rotateRight)
        {
            simpleRotation += (rotateRight) ? 1 : -1;
            baseRotation += (rotateRight) ? 1 : -1;
            if (simpleRotation < 0) simpleRotation += 4;
            if (baseRotation < 0) baseRotation += 4;
            simpleRotation = simpleRotation % 4;
            baseRotation = baseRotation % 4;
            rotation = simpleRotation * Microsoft.Xna.Framework.MathHelper.PiOver2;
            baseActualRotation = baseRotation * Microsoft.Xna.Framework.MathHelper.PiOver2;
            
            //rotation += ((rotateRight) ? Microsoft.Xna.Framework.MathHelper.PiOver2 : -Microsoft.Xna.Framework.MathHelper.PiOver2); // 90 : -90); 
            //if (rotation < 0) rotation += Microsoft.Xna.Framework.MathHelper.Pi*2;
            //while (rotation > Microsoft.Xna.Framework.MathHelper.Pi * 2) rotation -= Microsoft.Xna.Framework.MathHelper.Pi * 2;
            //rotation = rotation % Microsoft.Xna.Framework.MathHelper.Pi*2;
            return rotation;
        }

        public void setLocation(Point centrePos)
        {
            if (!selected) return;
            curPoint.X = centrePos.X; //-width / 2;
            curPoint.Y = centrePos.Y; //-height / 2;
            dest = new Rectangle(curPoint.X, curPoint.Y, width, height);
        }

        public bool isPointInThis(Point p)
        {
            return selectionBox.Contains(p);
        }

        public Rectangle getBoundBox()
        {
            return selectionBox;
        }

        public void setSelected(bool selected)
        {
            this.selected = selected;
        }

        public bool isSelected()
        {
            return selected;
        }

        public void setGridRef(int gridRef)
        {
            this.gridRef = gridRef;
            gridPoint = tilePuzzle.getGridPointbyRef(gridRef);
            curPoint = tilePuzzle.getGridPointbyRef(gridRef);
            //dest = new Rectangle(curPoint.X + width / 2 + 3/*- 17*/, curPoint.Y + height / 2 + 3/*- 17*/, width, height);
            dest = new Rectangle(gridPoint.X + width / 2 /*-25*/, gridPoint.Y + height / 2 /*-25*/, width, height);
            selectionBox = new Rectangle(gridPoint.X, gridPoint.Y, width, height);
        }

        public int getGridRef()
        {
            return gridRef;
        }

        public void setFadeValue(float fadeValue)
        {
            this.fadeValue = fadeValue;
        }

        public bool isSolution()
        {
            return simpleRotation == 0 && gridRef == solGridRef;
        }
    }
}
