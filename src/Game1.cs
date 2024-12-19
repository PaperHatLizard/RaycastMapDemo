using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace RaycastMapDemo
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;
        private FrameCounter _frameCounter = new FrameCounter();
        private Effect _screenShader;
        private Texture2D screenTexture;
        private Player player;
        private Map map;
        private ShaderDebug shaderDebug;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            map = new Map(20,20);
            player = new Player(10, 10, 0, map);

            shaderDebug = new ShaderDebug(player, map, GraphicsDevice);

            SamplerState borderSampler = new SamplerState
            {
                AddressU = TextureAddressMode.Border,
                AddressV = TextureAddressMode.Border,
                AddressW = TextureAddressMode.Border,
                BorderColor = Color.Magenta,
                Filter = TextureFilter.Point
            };

            GraphicsDevice.SamplerStates[0] = borderSampler;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteFont = Content.Load<SpriteFont>("font");
            _screenShader = Content.Load<Effect>("ScreenShader");

            int width = GraphicsDevice.Viewport.Width;
            int height = GraphicsDevice.Viewport.Height;
            screenTexture = new Texture2D(GraphicsDevice, width, height);

            Color[] data = new Color[width * height];
            Array.Fill(data, Color.Orange);
            screenTexture.SetData(data);

        }

        int lastPressCount = 0;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            player.Update(gameTime);

            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.GetPressedKeyCount() != lastPressCount)
            {
                if (!keyboardState.IsKeyDown(Keys.LeftShift))
                {
                    if (keyboardState.IsKeyDown(Keys.X))
                        MapSamplePoint.X++;
                    if (keyboardState.IsKeyDown(Keys.Y))
                        MapSamplePoint.Y++;
                }
                else
                {
                    if (keyboardState.IsKeyDown(Keys.X))
                        MapSamplePoint.X--;
                    if (keyboardState.IsKeyDown(Keys.Y))
                        MapSamplePoint.Y--;
                }

                if (keyboardState.IsKeyDown(Keys.Tab))
                {
                    DrawDebug = !DrawDebug;
                }

                lastPressCount = keyboardState.GetPressedKeyCount();
            }


            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        bool DrawDebug = false;

        Vector2 MapSamplePoint = new Vector2(0, 0);

        protected override void Draw(GameTime gameTime)
        {
            DrawGame(gameTime);
            if (DrawDebug)
                shaderDebug.Draw();
        }

        public void DrawGame(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Texture2D mapTex = map.GetMapTexture(GraphicsDevice, 1, false);
            Vector2 screenSize = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            _screenShader.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            _screenShader.Parameters["ScreenSize"]?.SetValue(screenSize);
            _screenShader.Parameters["MapTexture"]?.SetValue(mapTex);
            _screenShader.Parameters["MapSize"]?.SetValue(new Vector2(map.Width, map.Height));
            _screenShader.Parameters["PlayerPosition"]?.SetValue(new Vector2(player.X, player.Y));
            _screenShader.Parameters["PlayerRotation"]?.SetValue(player.Rotation);
            _screenShader.Parameters["MapSamplePoint"]?.SetValue(MapSamplePoint);
            _screenShader.CurrentTechnique.Passes[0].Apply();


            _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, _screenShader);

            //Draw sprite to cover the whole screen for shader to apply properly
            _spriteBatch.Draw(screenTexture, Vector2.Zero, Color.White);


            _spriteBatch.End();

            _frameCounter.Update(deltaTime);

            _spriteBatch.Begin();

            var fps = string.Format("FPS: {0}", _frameCounter.AverageFramesPerSecond);

            _spriteBatch.Draw(map.GetMapTexture(GraphicsDevice, 10), new Vector2(0, 0), Color.White);
            //_spriteBatch.Draw(_screenShader.Parameters["MapTexture"].GetValueTexture2D(), new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2), Color.White);

            //_spriteBatch.Draw(mapTex, new Vector2(GraphicsDevice.Viewport.Width/2, GraphicsDevice.Viewport.Height/2), Color.White);

            _spriteBatch.DrawString(_spriteFont, fps, new Vector2(1, 1), Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
