using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class StreetMap
    {
        public enum ObjType { Nothing = 0, AIPath = 1, AIStop= 2, Obstacle = 3, LevelEnd = 4 };
        public enum Direction { Left, Right, Up, Stop };

        private ObjType[,] map;
        private int lastRowId;
        private int pathID;
        private int pathCell;

        private int COLS;
        private int ROWS;

        private List<Texture2D> mainSprites;
        private List<Texture2D> collidedSprites;

        private int MAXCOLS = 5;
        private Random gen;
        private Game1 appRef;

        public StreetMap(Game1 appRef)
        {
            this.appRef = appRef;
            gen = new Random();
            map = generateMap(100, MAXCOLS);
            lastRowId = 100;
            pathID = 0;
            pathCell = 1;//(int)Math.Ceiling((double) COLS / 2);

            mainSprites = new List<Texture2D>();
            collidedSprites = new List<Texture2D>();

            loadSprites();
        }

        private void loadSprites()
        {
            // Main Sprites:
            // 0 = Crate
            // 1 = Market Stall Green Left
            // 2 = Market Stall Green Right
            // 3 = Market Stall Red Left
            // 4 = Market Stall Red Right
            // 5 = Market Stall Yellow Left
            // 6 = Market Stall Yellow Right
            // 7 = River
            // 8 = River Crossing
            // 9 = Person for people object
            // 10 = Speech bubble
            // 11 = Path Helper Background (Debug only sprite)

            mainSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//Box"));
            mainSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//Stallgreenleft"));
            mainSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//Stallgreenright"));
            mainSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//Stallredleft"));
            mainSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//Stallredright"));
            mainSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//Stallyellowleft"));
            mainSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//Stallyellowright"));
            mainSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//rivertile"));
            mainSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//bridge")); 
            mainSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//charother"));
            mainSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//SpeechNormal"));
            mainSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//pathHelperBackground"));

            // Main Sprites:
            // 0 = Crate Broken
            // 1 = Market Stall Green Left Broken
            // 2 = Market Stall Green Right Broken
            // 3 = Market Stall Red Left Broken
            // 4 = Market Stall Red Right Broken
            // 5 = Market Stall Yellow Left Broken
            // 6 = Market Stall Yellow Right Broken
            // 7 = River Broken (Splashed)
            // 8 = Speech bubble Cursing

            collidedSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//BoxBroken"));
            collidedSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//Stallgreenbrokenleft"));
            collidedSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//Stallgreenbrokenright"));
            collidedSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//Stallredbrokenleft"));
            collidedSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//Stallredbrokenright"));
            collidedSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//Stallyellowbrokenleft"));
            collidedSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//Stallyellowbrokenright"));
            collidedSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//rivertile"));
            collidedSprites.Add(appRef.Content.Load<Texture2D>("StreetPuzzle//SpeechCollided"));
        }

        public ObjType[,] getMap()
        {
            return map;
        }

        public List<ObstacleObj> getNextRow(int startX, int startY, int objWidth, int objHeight)
        {
            List<ObstacleObj> result = new List<ObstacleObj>();

            if (lastRowId == 0)
            {
                for (int col = 0; col < COLS; col++)
                {
                    Rectangle dest = new Rectangle(startX + col * objWidth, startY, objWidth, objHeight);
                    result.Add(new PathHelper(ObstacleObj.ObstacleType.LevelEnd, -1, PathHelper.Direction.Up, dest, mainSprites[1]));
                    //result.Add(new PathHelper(ObstacleObj.ObstacleType.PathStop, pathID++, PathHelper.Direction.Right, dest, mainSprites[11]));
                }
                return result;
            }

            lastRowId--;
            // handle any pathing in this row (assumes max of 1 translation per row)
            if (pathCell != 0 
                 && (map[lastRowId, pathCell - 1] == ObjType.AIPath 
                    || map[lastRowId, pathCell - 1] == ObjType.AIStop))
            {
                Rectangle dest = new Rectangle(startX + pathCell * objWidth, startY, objWidth, objHeight);
                result.Add(getPathElement(dest, PathHelper.Direction.Left));
                pathCell--;
                dest = new Rectangle(startX + pathCell * objWidth, startY, objWidth, objHeight);
                result.Add(getPathElement(dest, PathHelper.Direction.Up));
            }
            else if (pathCell != COLS - 1
                    && (map[lastRowId, pathCell + 1] == ObjType.AIPath
                    || map[lastRowId, pathCell + 1] == ObjType.AIStop))
            {
                Rectangle dest = new Rectangle(startX + pathCell * objWidth, startY, objWidth, objHeight);
                result.Add(getPathElement(dest, PathHelper.Direction.Right));
                pathCell++;
                dest = new Rectangle(startX + pathCell * objWidth, startY, objWidth, objHeight);
                result.Add(getPathElement(dest, PathHelper.Direction.Up));
            }
            else
            {
                Rectangle dest = new Rectangle(startX + pathCell * objWidth, startY, objWidth, objHeight);
                result.Add(getPathElement(dest, PathHelper.Direction.Up));
            }

            // select if this row will be displayed as a river crossing instead
            bool riverrow = true;
            int pathcount = 1;
            for (int col = 0; col < COLS; col++)
            {
                if (map[lastRowId, col] != ObjType.Obstacle && map[lastRowId, col] != ObjType.AIPath
                    && map[lastRowId, col] != ObjType.AIStop)
                {
                    riverrow = false;
                    break;
                }

                // handle exception where the AI moves multiple squares on the same row
                if (map[lastRowId, col] == ObjType.AIPath || map[lastRowId, col] == ObjType.AIStop)
                {
                    pathcount--;
                    if (pathcount < 0)
                    {
                        riverrow = false;
                        break;
                    }
                }
            }

            if (riverrow && gen.NextDouble() > 0.1)
            {
                riverrow = false;
            }

            // handle inserting all remaining row elements
            for (int col = 0; col < COLS; col++)
            {
                Rectangle dest = new Rectangle(startX + col * objWidth, startY, objWidth, objHeight);
                if (map[lastRowId, col] == ObjType.Obstacle)
                {
                    if (riverrow)
                    {
                        // insert plain river object
                        result.Add(new ObstacleObj(ObstacleObj.ObstacleType.Crate, dest,
                                            mainSprites[7], collidedSprites[7]));
                    }
                    else
                    {
                        result.Add(getObstacle(col, dest));
                    }
                }
                else if (riverrow && (map[lastRowId, col] == ObjType.AIPath || riverrow && map[lastRowId, col] == ObjType.AIStop))
                {
                    // insert river crossing
                    result.Add(new ObstacleObj(ObstacleObj.ObstacleType.RiverOverpass, dest,
                                        mainSprites[8], collidedSprites[8]));
                }
            }

            return result;
        }

        public bool allRowsRetrieved()
        {
            return lastRowId == 0;
        }

        private PathHelper getPathElement(Rectangle dest, PathHelper.Direction dir)
        {
            if (map[lastRowId, pathCell] == ObjType.AIPath)
            {
                return new PathHelper(ObstacleObj.ObstacleType.Path, pathID++, dir, dest, mainSprites[11]);
            }
            else if (map[lastRowId, pathCell] == ObjType.AIStop)
            {
                return new PathHelper(ObstacleObj.ObstacleType.PathStop, pathID++, dir, dest, mainSprites[11]);
            }

            // never reached, but must be here to remove errors
            return null;
        }

        private ObstacleObj getObstacle(int col, Rectangle dest)
        {
            /* 
             * when on edge:
             * 72% stall
             * 24% crate
             * 4% people
             * 
             * When in middle:
             * 40% people (originally 10% vs 90%)
             * 60% crate
             */
            double rand = gen.NextDouble();

            if (col == 0 || col == MAXCOLS - 1)
            {
                // on edge

                if (rand < 0.24)
                {
                    // green stall
                    int spriteIndex = (col == 0) ? 1 : 2;

                    return new ObstacleObj(ObstacleObj.ObstacleType.MarketStall, dest,
                                            mainSprites[spriteIndex], collidedSprites[spriteIndex]);
                }
                else if (rand < 0.48)
                {
                    // red stall
                    int spriteIndex = (col == 0) ? 3 : 4;

                    return new ObstacleObj(ObstacleObj.ObstacleType.MarketStall, dest,
                                            mainSprites[spriteIndex], collidedSprites[spriteIndex]);
                }
                else if (rand < 0.72)
                {
                    // yellow stall
                    int spriteIndex = (col == 0) ? 5 : 6;

                    return new ObstacleObj(ObstacleObj.ObstacleType.MarketStall, dest,
                                            mainSprites[spriteIndex], collidedSprites[spriteIndex]);
                }
                else if (rand < 0.96)
                {
                    // crate
                    return new ObstacleObj(ObstacleObj.ObstacleType.Crate, dest,
                                            mainSprites[0], collidedSprites[0]);
                }
                else
                {
                    // people
                    // (sprites main: 9, 10; collided: 8)
                    return new PeopleObstacle(dest, mainSprites[9], mainSprites[10], collidedSprites[8], mainSprites[11], gen);
                    //return new ObstacleObj(ObstacleObj.ObstacleType.Crate, dest,
                    //                        mainSprites[0], collidedSprites[0]);
                }
            }
            else
            {
                if (rand < 0.4)
                {
                    // people
                    // (sprites main: 9, 10; collided: 8)
                    return new PeopleObstacle(dest, mainSprites[9], mainSprites[10], collidedSprites[8], mainSprites[11], gen);
                    //return new ObstacleObj(ObstacleObj.ObstacleType.Crate, dest,
                    //                        mainSprites[0], collidedSprites[0]);
                }
                else
                {
                    // crate
                    return new ObstacleObj(ObstacleObj.ObstacleType.Crate, dest,
                                            mainSprites[0], collidedSprites[0]);
                }
            }
        }

        private ObjType[,] generateMap(int rows, int cols)
        {
            this.ROWS = rows;
            this.COLS = cols;
            ObjType[,] result = new ObjType[rows,cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = ObjType.Nothing;
                }
            }

            // Generate a path
            int pathIndex = 1;//(int)Math.Ceiling((double) cols / 2);
            int stopModifier = 20; 
            for (int i = rows-1; i >= 0; i--)
            {
                result[i, pathIndex] = (gen.Next(0,stopModifier) == 0) ? ObjType.AIStop : ObjType.AIPath;

                if (gen.Next(0, 6) == 0)
                {
                    if (pathIndex == 0) // && (result[i, pathIndex+1] != ObjType.AIStop || result[i, pathIndex+1] != ObjType.AIPath))
                    {
                        pathIndex++;
                    }
                    else if (pathIndex == cols - 1) // && (result[i, pathIndex-1] != ObjType.AIStop || result[i, pathIndex-1] != ObjType.AIPath)
                    {
                        pathIndex--;
                    }
                    else // if (pathIndex != cols - 1 && pathIndex != 0)
                    {
                        pathIndex += (gen.Next(0, 2) == 0) ? -1 : 1;
                    }
                    result[i, pathIndex] = (gen.Next(0, stopModifier) == 0) ? ObjType.AIStop : ObjType.AIPath;
                }
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (result[i, j] == ObjType.Nothing && gen.NextDouble() <= 0.7)
                    {
                        result[i, j] = ObjType.Obstacle;
                    }
                }
            }

            string mapData = "";
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    //Console.Write((int)result[i,j]);
                    mapData += (int)result[i, j];
                    if (j != cols - 1)
                    {
                        mapData += ",";
                    }
                }
                //Console.WriteLine();
                mapData += ";";
            }
            appRef.insertLog(DataLog.DataType.Misc, DataElement.DataType.PuzzleData, "Map Data: " + mapData);

            return result;
        }
    }
}
