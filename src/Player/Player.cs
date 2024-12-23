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
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Rotation { get; private set; }
        public float Speed = 5f;
        private Map map;
        private Vector2 mousePosition;

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

        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            Vector2 movement = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.W))
            {
                movement.X = 1;
            }

            if (keyboardState.IsKeyDown(Keys.S))
            {
                movement.X = -1;
            }

            if (keyboardState.IsKeyDown(Keys.A))
            {
                movement.Y = -1;
            }

            if (keyboardState.IsKeyDown(Keys.D))
            {
                movement.Y = 1;
            }

            Move(movement.X, movement.Y, gameTime);

            Vector2 mouseDelta = mousePosition - new Vector2(mouseState.X, mouseState.Y);

            mousePosition = new Vector2(mouseState.X, mouseState.Y);

            Rotation -= mouseDelta.X * 0.01f;

            //Clamp rotation
            if (Rotation >= Math.Tau)
                Rotation = 0;
            else if (Rotation < 0)
                Rotation = (float)(Math.Tau);

        }

        /// <summary>
        /// Moves the player based on the input direction and game time.
        /// </summary>
        /// <param name="dx">The x-direction input.</param>
        /// <param name="dy">The y-direction input.</param>
        /// <param name="gameTime">The game time for calculating movement delta.</param>
        public void Move(float dx, float dy, GameTime gameTime)
        {
            // Get time delta for smooth movement
            float delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000;

            // Calculate velocity based on direction and speed
            float velX = dx * Speed * delta;
            float velY = dy * Speed * delta;

            // Apply the movement, translating the position based on rotation
            Vector2 velocity = new Vector2(velX, velY);
            // Translate the velocity based on the player's rotation
            Vector2 rotated = Vector2.Transform(velocity, Matrix.CreateRotationZ(Rotation));
            velocity = rotated;

            //Calculate collisions with walls
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

            //Round to correct for the float precision on the GPU calculations,
            //Jittering occurs otherwise
            X = (float)Math.Round(X, 2);
            Y = (float)Math.Round(Y, 2);
        }

        /// <summary>
        /// Calculate the normal of the wall based on the player's position and velocity.
        /// </summary>
        /// <param name="wallPosition">The position of the wall that the player is colliding with.</param>
        /// <param name="velocity">The current velocity of the player.</param>
        /// <returns>A Vector2 representing the normal of the wall.</returns>
        public Vector2 CalculateWallNormal(Vector2 wallPosition, Vector2 velocity)
        {
            // Assuming the wall is axis-aligned, we can determine the normal based on the position
            Vector2 normal = velocity;

            // Check the horizontal and vertical boundaries
            if (map.GetMapAt((int)wallPosition.X, (int)Y) != 0)
            {
                normal.X = -velocity.X * 0.2f;
            }
            if (map.GetMapAt((int)X, (int)wallPosition.Y) != 0)
            {
                normal.Y = -velocity.Y * 0.2f;
            }

            return normal;
        }

    }
}
