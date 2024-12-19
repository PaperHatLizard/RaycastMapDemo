using System;
using System.Collections.Generic;
using System.Data.Common;
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

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    {
                        

                        if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                        {
                            SetMapAt(x, y, 1);
                        }
                        else
                        {
                            SetMapAt(x, y, 0);
                        }
                    }
                }
            }

            SetMapAt(4, 4, 1);
            SetMapAt(3, 2, 1);
            SetMapAt(5, 4, 1);
            SetMapAt(4, 7, 1);
            SetMapAt(16, 4, 1);
            SetMapAt(18, 5, 1);

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


            float angle = Player.Instance.Rotation;

            Vector2 direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            Vector2 endPosition = playerPos + (direction);

            int playerMapValue = GetMapAt((int)playerPos.X, (int)playerPos.Y);
            int playerLookValue = GetMapAt((int)endPosition.X, (int)endPosition.Y);

            if (showPlayer)
            {
                SetMapAt((int)endPosition.X, (int)endPosition.Y, -3);
                SetMapAt((int)playerPos.X, (int)playerPos.Y, -2);
            }
            Texture2D mapTex = new Texture2D(graphics, Width * scale, Height * scale);

            Color[] pixels = new Color[Width * Height * scale * scale];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Color color = map[x, y] == 0 ? Color.White : Color.Black;
                    if (map[x,y] == -2)
                    {
                        color = playerColor;
                    }
                    if (map[x,y] == -3)
                    {
                        color = Color.Blue;
                    }
                    if (map[x,y] == -4)
                    {
                        color = Color.Yellow;
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
            SetMapAt((int)endPosition.X, (int)endPosition.Y, playerLookValue);

            mapTex.SetData(pixels);

            return mapTex;
        }
    }
}
