using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RaycastMapDemo
{
    public class Map
    {
        public int Width = 10;
        public int Height = 10;

        private int[,] map;

        private Color floorColor;
        private Color ceilingColor;
        private Color wallColor;
        private Color playerColor;

        private List<Texture2D> wallTextures;
        private int[] wallIdsUsed;

        public Color FloorColor { get => floorColor; }
        public Color CeilingColor { get => ceilingColor; }
        public Color WallColor { get => wallColor; }

        public Map(int width, int height) 
        {
            Width = width;
            Height = height;

            map = new int[Width, Height];

            floorColor = Color.Gray;
            ceilingColor = Color.CornflowerBlue;
            wallColor = Color.Red;
            playerColor = Color.Green;

        }

        public void GenerateMap(int seed = 1337)
        {
            Random rand = new Random(seed);

            List<int> wallIds = new List<int>();

            //Iterate through the map and generate walls at random
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                    {
                        SetMapAt(x, y, 1);
                    }
                    else
                    {
                        int wallEmpty = rand.Next(0, 11);
                        if (wallEmpty <= 7) continue;

                        int wallID = rand.Next(1, wallTextures.Count+1);

                        if (!wallIds.Contains(wallID))
                        {
                            wallIds.Add(wallID);
                        }

                        SetMapAt(x, y, wallID);
                    }
                }
            }

            Vector2 playerPos = new Vector2(Player.Instance.X, Player.Instance.Y);

            //Clear the area around the player
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    SetMapAt((int)playerPos.X + x, (int)playerPos.Y + y, 0);
                }
            }

            wallIdsUsed = wallIds.ToArray();
        }

        public void SetMapWallTextures(List<Texture2D> textures)
        {
            Debug.WriteLine("Total textures: " + textures.Count); 
            wallTextures = textures;
        }

        public void SetMapAt(int x, int y, int value)
        {
            map[x, y] = value;
        }

        public int GetMapAt(int x, int y)
        {
            if (x > Width || y > Height || x < 0 || y < 0)
            {
                return -1;
            }
            return map[x, y];
        }
        /// <summary>
        /// Generates a texture representation of the map.
        /// </summary>
        /// <param name="graphics">The graphics device to create the texture.</param>
        /// <param name="scale">The scale factor for each map cell.</param>
        /// <returns>A Texture2D object representing the map.</returns>
        public Texture2D GetMapTexture(GraphicsDevice graphics, int scale, bool showPlayer = true)
        {

            Vector2 playerPos = new Vector2(Player.Instance.X, Player.Instance.Y);

            int playerMapValue = GetMapAt((int)playerPos.X, (int)playerPos.Y);

            //For our debug view and map view, we want to show the player and the direction they are looking
            if (showPlayer)
            {
                SetMapAt((int)playerPos.X, (int)playerPos.Y, -2);
            }

            Texture2D mapTex = new Texture2D(graphics, Width * scale, Height * scale);

            Color[] pixels = new Color[Width * Height * scale * scale];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Color color = Color.White;

                    float mapID = GetMapAt(x, y);
                    //Could later implement g and b for further array sizing
                    //when b iterates +1, reset r to 0, when g iterates +1, reset b to 0
                    //this would allow for a 3d "array" of mapIDs, potentially 16,777,216? if my math is correct
                    //Unsure how the screen shader will perform when mapID floats are super close to each other
                    float val = (mapID-1) / (wallIdsUsed.Count());

                    val = MathF.Round(val, 2);

                    if (mapID != 0)
                        color = new Color(val, 0, 0);


                    if (map[x,y] == -2)
                    {
                        color = playerColor;
                    }


                    for (int i = 0; i < scale; i++)
                    {
                        for (int j = 0; j < scale; j++)
                        {
                            pixels[(x * scale + i) + (y * scale + j) * Width * scale] = color;
                        }
                    }
                }
            }

            SetMapAt((int)playerPos.X, (int)playerPos.Y, playerMapValue);

            mapTex.SetData(pixels);

            return mapTex;
        }

        public Texture2D GenerateMapAtlasTexture(GraphicsDevice graphics)
        {
            Texture2D texture2D = new Texture2D(graphics, wallTextures[0].Width * wallIdsUsed.Length, wallTextures[1].Width);

            for (int i = 0; i < wallIdsUsed.Length; i++)
            {
                Color[] data = new Color[wallTextures[0].Width * wallTextures[0].Height];
                wallTextures[i].GetData(data);

                texture2D.SetData(0, new Rectangle(i * wallTextures[0].Width, 0, wallTextures[0].Width, wallTextures[0].Height), data, 0, data.Length);
            }

            return texture2D;
        }
    }
}
