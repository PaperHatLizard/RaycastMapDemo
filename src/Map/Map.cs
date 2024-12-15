using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

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

        public Color FloorColor { get => floorColor; }
        public Color CeilingColor { get => ceilingColor; }
        public Color WallColor { get => wallColor; }

        public Map(int width, int height) 
        {
            Width = width;
            Height = height;

            map = new int[Width+1, Height+1];

            floorColor = Color.Gray;
            ceilingColor = Color.CornflowerBlue;
            wallColor = Color.Red;

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
    }
}
