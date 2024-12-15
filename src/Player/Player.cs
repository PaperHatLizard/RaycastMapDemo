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
        public float X = 0;
        public float Y = 0;
        public float Rotation = 0;
        private Map map;
        public float Speed = 5f;

        public Player(float x, float y, float rotation, Map map)
        {
            X = x;
            Y = y;
            Rotation = rotation;
            this.map = map;
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

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
        }

        public void Move(float dx, float dy, GameTime gameTime)
        {
            float velX = X + dx;
            float velY = Y + dy;
            float delta = gameTime.ElapsedGameTime.Milliseconds / 1000f;
            //Disallow movement if there is a wall at the new position
            if (map.GetMapAt((int)velX, (int)velY) != 0)
                return;

            velX = velX * delta * Speed;
            velY = velY * delta * Speed;

            Debug.WriteLine("newX: " + velX + " newY: " + velY);

            X = X + velX;
            Y = Y + velY;
        }
    }
}
