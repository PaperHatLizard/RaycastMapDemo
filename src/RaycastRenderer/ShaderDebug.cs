using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace RaycastMapDemo
{
    public class ShaderDebug
    {
        private Player player;
        private Map map;
        private GraphicsDevice graphics;
        private SpriteBatch spriteBatch;

        private struct Line
        {
            public Vector2 Start;
            public Vector2 End;
            public Color Color;
        }

        public ShaderDebug(Player player, Map map, GraphicsDevice graphics)
        {
            this.player = player;
            this.map = map;
            this.graphics = graphics;
            spriteBatch = new SpriteBatch(graphics);
        }

        public void Draw()
        {
            int mapScale = 20;

            Texture2D mapTex = map.GetMapTexture(graphics, mapScale, true);

            spriteBatch.Begin();
            spriteBatch.Draw(mapTex, Vector2.Zero, Color.White);

            DrawFOV(mapScale);

            spriteBatch.End();
        }

        private void DrawFOV(int scale)
        {
            float columns = 20;

            float minAngleScale = 0;
            float maxAngleScale = 0;

            //simulate columns on screen
            for(int column = 0; column < columns; column++)
            {
                float angleScale = ((((float)column) / ((float)columns)) - 0.5f) * 8f;

                float b = MathHelper.ToDegrees(player.Rotation) + 90;

                b = MathHelper.ToRadians(b);

                Vector2 lineStart = new Vector2(player.X, player.Y);

                lineStart += new Vector2(MathF.Cos(b), MathF.Sin(b)) * angleScale;

                Vector2 lineEnd = new Vector2(player.X, player.Y) + new Vector2(MathF.Cos(player.Rotation), MathF.Sin(player.Rotation)) * 10;

                lineEnd += new Vector2(MathF.Cos(b), MathF.Sin(b)) * angleScale;

                if (column == 0)
                {
                    minAngleScale = angleScale;
                }
                if (column == columns - 1)
                {
                    maxAngleScale = angleScale;
                }

                Line line = new Line()
                {
                    Start = lineStart * scale,
                    End = lineEnd * scale,
                    Color = Color.Green
                };

                DrawLine(line, 2, spriteBatch);

                float lineAngle = MathF.Atan2(lineEnd.Y - lineStart.Y, lineEnd.X - lineStart.X);

                Bresenham(lineStart, lineAngle, scale, Color.Purple, Color.Orange);
            }

            

            Line playerDirection = new Line();
            playerDirection.Start = new Vector2(player.X, player.Y) * scale;
            playerDirection.End = (new Vector2(player.X, player.Y) + (new Vector2(MathF.Cos(player.Rotation), MathF.Sin(player.Rotation)) * 10)) * scale;
            playerDirection.Color = Color.Blue;

            DrawLine(playerDirection, 4, spriteBatch);
            Debug.WriteLine($"Minimum Angle Scale: {minAngleScale} Maximum Angle Scale: {maxAngleScale}");

        }

        private void Bresenham(Vector2 startPosition, float angle, int scale, Color lineColor, Color hitColor)
        {
            float dx = MathF.Cos(angle);
            float dy = MathF.Sin(angle);

            float xStep = MathF.Sign(dx);
            float yStep = MathF.Sign(dy);

            float xPos = startPosition.X;
            float yPos = startPosition.Y;

            float tMaxX = xStep > 0 ? (1.0f - Frac(xPos)) / dx : Frac(xPos) / -dx;
            float tMaxY = yStep > 0 ? (1.0f - Frac(yPos)) / dy : Frac(yPos) / -dy;

            float tDeltaX = 1.0f / MathF.Abs(dx);
            float tDeltaY = 1.0f / MathF.Abs(dy);

            int maxRayLength = 25;
            float lastSideHit = -1;

            for (int i = 0; i < maxRayLength; i++)
            {
                int mapValue = map.GetMapAt((int)xPos, (int)yPos);

                

                if (mapValue != 0)
                {
                    Line viewLine = new Line()
                    {
                        Start = startPosition * scale,
                        End = (startPosition + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * maxRayLength) * scale,
                        Color = lineColor
                    };

                    Line hitLine = CalculateIntersectionLine(viewLine, startPosition, new Vector2(xPos, yPos), scale, lastSideHit, hitColor);

                    DrawLine(hitLine, 2, spriteBatch);
                    DrawLine(viewLine, 2, spriteBatch);
                    return;
                }

                if (tMaxX < tMaxY)
                {
                    lastSideHit = dx > 0 ? 1 : 2;
                    tMaxX += tDeltaX;
                    xPos += xStep;
                }
                else
                {
                    lastSideHit = dy > 0 ? 3 : 4;
                    tMaxY += tDeltaY;
                    yPos += yStep;
                }


            }

            return;
        }


        private Line CalculateIntersectionLine(Line startLine, Vector2 start, Vector2 cell, int scale, float lastSideHit, Color color)
        {
            Vector2 intersectionStart, intersectionEnd;
            float cellSize = 0.5f;

            cell.X = MathF.Floor(cell.X) + 0.5f;
            cell.Y = MathF.Floor(cell.Y) + 0.5f;

            DrawCircle(cell * scale, 5, spriteBatch, Color.Red);

            // Determine the intersection line based on side hit
            if (lastSideHit == 1) // Left
            {
                intersectionStart = new Vector2(cell.X - cellSize, cell.Y - cellSize) * scale;
                intersectionEnd = new Vector2(cell.X - cellSize, cell.Y + cellSize) * scale;
            }
            else if (lastSideHit == 2) // Right
            {
                intersectionStart = new Vector2(cell.X + cellSize, cell.Y - cellSize) * scale;
                intersectionEnd = new Vector2(cell.X + cellSize, cell.Y + cellSize) * scale;
            }
            else if (lastSideHit == 3) // Top
            {
                intersectionStart = new Vector2(cell.X - cellSize, cell.Y - cellSize) * scale;
                intersectionEnd = new Vector2(cell.X + cellSize, cell.Y - cellSize) * scale;
            }
            else if (lastSideHit == 4)
            {
                intersectionStart = new Vector2(cell.X - cellSize, cell.Y + cellSize) * scale;
                intersectionEnd = new Vector2(cell.X + cellSize, cell.Y + cellSize) * scale;
            }
            else
                return new Line();

            Line intersectLine = new Line()
            {
                Start = intersectionStart,
                End = intersectionEnd,
                Color = color
            };

            float m1 = (startLine.End.Y - startLine.Start.Y) / (startLine.End.X - startLine.Start.X + 0.001f);
            float m2 = (intersectLine.End.Y - intersectLine.Start.Y) / (intersectLine.End.X - intersectLine.Start.X + 0.001f);

            //If slopes are nearly identical then these lines probably dont intersect
            if (MathF.Abs(m1 - m2) < 0.0001)
            {
                return new Line();
            }

            float b1 = startLine.Start.Y - m1 * startLine.Start.X;
            float b2 = intersectLine.Start.Y - m2 * intersectLine.Start.X;

            float x = (b2 - b1) / (m1 - m2);

            float y = m1 * x + b1;

            Vector2 intersectionPoint = new Vector2(x, y);

            float distance = Vector2.Distance(startLine.Start, intersectionPoint);

            Line distanceLine = new Line()
            {
                Start = startLine.Start,
                End = intersectionPoint,
                Color = Color.Yellow
            };

            DrawLine(distanceLine, 5, spriteBatch);
            
            return new Line()
            {
                Start = intersectionStart,
                End = intersectionEnd,
                Color = color
            };
        }

        private float Frac(float value)
        {
            return value - MathF.Floor(value);
        }

        private void DrawLine(Line line, int thickness, SpriteBatch spriteBatch)
        {
            int distance = (int)Vector2.Distance(line.Start, line.End);

            if (distance <= 0)
                return;

            Texture2D tex = new Texture2D(graphics, distance, thickness);

            Color[] data = new Color[distance * thickness];
            for (int i = 0; i < data.Length; i++)
                data[i] = line.Color;

            tex.SetData(data);

            float rotation = MathF.Atan2(line.End.Y - line.Start.Y, line.End.X - line.Start.X);
            Vector2 origin = new Vector2(0, thickness / 2f);

            spriteBatch.Draw(tex, line.Start, null, line.Color, rotation, origin, 1, SpriteEffects.None, 0);
        }


        private void DrawCircle(Vector2 position, int radius, SpriteBatch spriteBatch, Color color)
        {
            Texture2D texture = new Texture2D(graphics, radius, radius);
            Color[] colorData = new Color[radius * radius];

            float diam = radius / 2f;
            float diamsq = diam * diam;

            for (int x = 0; x < radius; x++)
            {
                for (int y = 0; y < radius; y++)
                {
                    int index = x * radius + y;
                    Vector2 pos = new Vector2(x - diam, y - diam);
                    if (pos.LengthSquared() <= diamsq)
                    {
                        colorData[index] = Color.White;
                    }
                    else
                    {
                        colorData[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(colorData);

            spriteBatch.Draw(texture, position, color);

        }
    }
}
