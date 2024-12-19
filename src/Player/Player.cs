using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaycastMapDemo
{
    public class Player
    {
        public static Player Instance { get; private set; }

        public float X = 0;
        public float Y = 0;
        public float Rotation { get; private set; }
        private Map map;
        public float Speed = 3f;

        public Player(float x, float y, float rotation, Map map)
        {
            if (Instance != null)
                return;

            Instance = this;

            X = x;
            Y = y;
            Rotation = rotation;
            this.map = map;
        }

        Vector2 mousePosition;

        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            Vector2 movement = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.W))
            {
                movement = new Vector2(MathF.Cos(Rotation), MathF.Sin(Rotation));
            }

            if (keyboardState.IsKeyDown(Keys.S))
            {
                movement = new Vector2(-MathF.Cos(Rotation), -MathF.Sin(Rotation));
            }

            if (keyboardState.IsKeyDown(Keys.A))
            {
                movement = new Vector2(MathF.Sin(Rotation), -MathF.Cos(Rotation));
            }

            if (keyboardState.IsKeyDown(Keys.D))
            {
                movement = new Vector2(-MathF.Sin(Rotation), MathF.Cos(Rotation));
            }

            Move(movement.X, movement.Y, gameTime);

            Vector2 mouseDelta = mousePosition - new Vector2(mouseState.X, mouseState.Y);

            mousePosition = new Vector2(mouseState.X, mouseState.Y);

            Rotation += mouseDelta.X * 0.01f;

            //Clamp rotation
            if (Rotation >= Math.Tau)
                Rotation = 0;
            else if (Rotation < 0)
                Rotation = (float)(Math.Tau);

        }

        public void Move(float dx, float dy, GameTime gameTime)
        {
            // Get time delta for smooth movement
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate velocity based on direction and speed
            float velX = dx * Speed * delta;
            float velY = dy * Speed * delta;

            // Apply the movement, translating the position based on rotation
            Vector2 velocity = new Vector2(velX, velY);

            // Translate the velocity based on the player's rotation

            Vector2 wallPosition = new Vector2((X + velocity.X), (Y + velocity.Y));

            int nextMapPosition = map.GetMapAt((int)wallPosition.X, (int)wallPosition.Y);

            if (nextMapPosition == 0)
            {
                X += velocity.X;
                Y += velocity.Y;
            }
            else
            {
                Vector2 normal = CalculateWallNormal(wallPosition, velocity);

                velocity = normal;

                X += velocity.X;
                Y += velocity.Y;
            }

            X = (float)Math.Round(X, 2);
            Y = (float)Math.Round(Y, 2);
        }

        public Vector2 CalculateWallNormal(Vector2 wallPosition, Vector2 velocity)
        {
            // Assuming the wall is axis-aligned, we can determine the normal based on the position
            Vector2 normal = velocity;

            // Check the horizontal and vertical boundaries
            if (map.GetMapAt((int)wallPosition.X, (int)Y) != 0)
            {
                normal.X = -velocity.X * 0.001f;
            }
            if (map.GetMapAt((int)X, (int)wallPosition.Y) != 0)
            {
                normal.Y = -velocity.Y * 0.001f;
            }

            return normal;
        }




    }
}
