// C# code for DDA line generation 
// Code referenced from https://www.geeksforgeeks.org/dda-line-generation-algorithm-computer-graphics/
// Modified to fit the RaycastRenderer class

using System;
namespace RaycastMapDemo
{
    public static class DDA
    {
        public static int Round(float n)
        {
            if (n - (int)n < 0.5)
                return (int)n;
            return (int)(n + 1);
        }

        /// <summary>
        /// Performs DDA line generation to simulate raycasting in a 2D grid map.
        /// </summary>
        /// <param name="x0">The initial x position</param>
        /// <param name="y0">The initial y position</param>
        /// <param name="angle">Angle in degrees</param>
        /// <param name="maximumDistance">The maximum distance the ray will travel</param>
        /// <param name="map">The map to raycast</param>
        /// <returns>The distance to the first wall hit, or -1 if no wall is hit within the maximum distance</returns>
        public static float DDALine(int x0, int y0, float angle, int maximumDistance, Map map)
        {
            // Convert to radians
            angle = angle * (float)(Math.PI / 180);

            int x1 = x0 + (int)(maximumDistance * Math.Cos(angle));
            int y1 = y0 + (int)(maximumDistance * Math.Sin(angle));

            // Calculate dx and dy 
            int dx = x1 - x0;
            int dy = y1 - y0;

            int step;

            // If dx > dy we will take step as dx 
            // else we will take step as dy to draw the complete 
            // line 
            if (Math.Abs(dx) > Math.Abs(dy))
                step = Math.Abs(dx);
            else
                step = Math.Abs(dy);

            // Calculate x-increment and y-increment for each 
            // step 
            float x_incr = (float)dx / step;
            float y_incr = (float)dy / step;

            // Take the initial points as x and y 
            float x = x0;
            float y = y0;

            for (int i = 0; i < step; i++)
            {
                x += x_incr;
                y += y_incr;
                int hit = map.GetMapAt(Round(x), Round(y));
                //if hit a wall, return distance
                if (hit != 0 && hit != -1)
                {
                    return (float)Math.Sqrt(Math.Pow(x - x0, 2) + Math.Pow(y - y0, 2));
                }
            }

            return -1;
        }
    }
}
