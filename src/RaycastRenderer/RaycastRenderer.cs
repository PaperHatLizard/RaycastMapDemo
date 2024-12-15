using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaycastMapDemo
{
    public class RaycastRenderer : Texture2D
    {
        private Color[] pixels;
        private Map map;
        private Player player;
        private int FOV = 60;


        public RaycastRenderer(GraphicsDevice graphicsDevice, int width, int height, Map map, Player player) : base(graphicsDevice, width, height) 
        {
            this.pixels = new Color[width * height];
            this.map = map;
            this.player = player;
        }

        public void Draw(GraphicsDevice graphicsDevice, GameTime time)
        {
            Array.Clear(pixels);

            //Draws from top left corner to bottom right corner
            //Draws column by column
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (y < Height / 2)
                    {
                        SetPixel(x, y, map.CeilingColor);
                    }
                    else
                    {
                        SetPixel(x, y, map.FloorColor);
                    }
                }

                DrawLine(x);
            }

            SetData(pixels);
        }

        public void DrawLine(int col)
        {
            int size = Height;

            float angle = player.Rotation - (FOV / 2) + (col * FOV / Width);

            float distance = DDA.DDALine(
                (int)player.X, (int)player.Y, angle, 100, map);

            if (distance == -1) return;

            int lineHeight = (int)(size / distance);

            int start = (size / 2) - (lineHeight / 2);
            int end = start + lineHeight;

            for (int i = start; i < end; i++)
            {
                SetPixel(col, i, map.WallColor);

            }
        }


        /// <summary>
        /// Sets the color buffer to be used to mass set the texture later as to not
        /// create Width * Height new Color objects every frame
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        private void SetPixel(int x, int y, Color color)
        {
            try
            {
                pixels[FlattenArray(x, y)] = color;
            }
            catch (IndexOutOfRangeException)
            {
                //TODO: If error occurs, pixels array may need to be resized
            }
        }

        /// <summary>
        /// A flattening function to convert 2D screen coordinates to 1D array coordinates
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>Returns the index of the 1D array that represents the 2D array</returns>
        private int FlattenArray(int x, int y)
        {
            return y * Width + x;
        }
    }
}
