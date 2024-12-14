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
        public Color[] pixels;

        public RaycastRenderer(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height) 
        {
            pixels = new Color[width * height];
        }

        public void Draw(GraphicsDevice graphicsDevice, GameTime time)
        {
            Array.Clear(pixels);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    SetPixel(x, y, Color.Black);
                }
            }


            SetData(pixels);
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
