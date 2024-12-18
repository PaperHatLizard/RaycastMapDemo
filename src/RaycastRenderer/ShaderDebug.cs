using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

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
            int mapScale = 10;

            Texture2D mapTex = map.GetMapTexture(graphics, mapScale, true);

            spriteBatch.Begin();
            spriteBatch.Draw(mapTex, Vector2.Zero, Color.White);

            Bresenham(new Vector2(player.X, player.Y), player.Rotation, mapScale);

            spriteBatch.End();
        }

        private void Bresenham(Vector2 startPosition, float angle, int scale)
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

            int maxRayLength = 30;
            float lastSideHit = -1;

            for (int i = 0; i < maxRayLength; i++)
            {
                int mapValue = map.GetMapAt((int)xPos, (int)yPos);

                if (mapValue != 0)
                {

                    Line hitLine = CalculateIntersectionLine(startPosition, new Vector2(xPos, yPos), scale, lastSideHit);

                    Line viewLine = new Line()
                    {
                        Start = startPosition * scale,
                        End = new Vector2(xPos, yPos) * scale,
                        Color = Color.Green
                    };

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

        private Line CalculateIntersectionLine(Vector2 start, Vector2 cell, int scale, float lastSideHit)
        {
            Vector2 intersectionStart, intersectionEnd;
            float cellSize = 0.5f;

            cell.X = MathF.Floor(cell.X) + 0.5f;
            cell.Y = MathF.Floor(cell.Y) + 0.5f;

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
            else // Bottom
            {
                intersectionStart = new Vector2(cell.X - cellSize, cell.Y + cellSize) * scale;
                intersectionEnd = new Vector2(cell.X + cellSize, cell.Y + cellSize) * scale;
            }

            return new Line()
            {
                Start = intersectionStart,
                End = intersectionEnd,
                Color = Color.Red
            };
        }

        private float Frac(float value)
        {
            return value - MathF.Floor(value);
        }

        private void DrawLine(Line line, int thickness, SpriteBatch spriteBatch)
        {
            int distance = (int)Vector2.Distance(line.Start, line.End);
            Texture2D tex = new Texture2D(graphics, distance, thickness);

            Color[] data = new Color[distance * thickness];
            for (int i = 0; i < data.Length; i++)
                data[i] = line.Color;

            tex.SetData(data);

            float rotation = MathF.Atan2(line.End.Y - line.Start.Y, line.End.X - line.Start.X);
            Vector2 origin = new Vector2(0, thickness / 2f);

            spriteBatch.Draw(tex, line.Start, null, line.Color, rotation, origin, 1, SpriteEffects.None, 0);
        }
    }
}
