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

            if (keyboardState.IsKeyDown(Keys.W))
            {
                Move(MathF.Cos(Rotation), MathF.Sin(Rotation), gameTime);
            }

            if (keyboardState.IsKeyDown(Keys.S))
            {
                Move(-MathF.Cos(Rotation), -MathF.Sin(Rotation), gameTime);
            }

            if (keyboardState.IsKeyDown(Keys.A))
            {
                Move(-MathF.Sin(Rotation), MathF.Cos(Rotation), gameTime);
            }

            if (keyboardState.IsKeyDown(Keys.D))
            {
                Move(MathF.Sin(Rotation), -MathF.Cos(Rotation), gameTime);
            }

            if (keyboardState.IsKeyDown(Keys.J))
            {
                this.Y = 6.9988f;
            }
            if (keyboardState.IsKeyDown(Keys.K))
            {
                this.Y = 7.00000001f;
            }

            Vector2 mouseDelta = mousePosition - new Vector2(mouseState.X, mouseState.Y);

            mousePosition = new Vector2(mouseState.X, mouseState.Y);

            Rotation += mouseDelta.X * 0.01f;

            //Clamp rotation
            if (Rotation >= Math.PI * 2)
                Rotation = 0;
            else if (Rotation < 0)
                Rotation = (float)(Math.PI * 2);

        }

        public void Move(float dx, float dy, GameTime gameTime)
        {
            // Get time delta for smooth movement
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate velocity based on direction and speed
            float velX = dx * Speed * delta;
            float velY = dy * Speed * delta;

            // Calculate the predicted future position after movement
            float newX = X + velX;
            float newY = Y + velY;

            // Check for collision at both the current and predicted positions
            int currentMapValue = map.GetMapAt((int)Math.Round(X), (int)Math.Round(Y));
            int newMapValue = map.GetMapAt((int)Math.Round(newX), (int)Math.Round(newY));

            Debug.WriteLine("CurrentX: " + X + " CurrentY: " + Y);
            Debug.WriteLine("newX: " + newX + " newY: " + newY);
            Debug.WriteLine("currentMapValue: " + currentMapValue + " newMapValue: " + newMapValue);

            // Disallow movement if there is a wall at the current or next position
            //if (currentMapValue != 0 || newMapValue != 0)
            //    return;

            // Apply the movement, translating the position based on rotation
            Vector2 velocity = new Vector2(velX, velY);

            // Translate the velocity based on the player's rotation
            Vector2 translatedMovement;

            translatedMovement.X = velocity.X * MathF.Cos(Rotation) + velocity.Y * MathF.Sin(Rotation);
            translatedMovement.Y = -velocity.X * MathF.Sin(Rotation) + velocity.Y * MathF.Cos(Rotation);

            // Update player position
            X += translatedMovement.X;
            Y += translatedMovement.Y;

            X = (float)Math.Round(X, 2);
            Y = (float)Math.Round(Y, 2);
        }


    }
}
