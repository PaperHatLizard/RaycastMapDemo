using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
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
        private List<Texture2D> wallTextures = new List<Texture2D>();
        int lastPressCount = 0;
        bool DrawDebug = false;
        Vector2 MapSamplePoint = new Vector2(0, 0);

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
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

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1 / 120.0f);

            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteFont = Content.Load<SpriteFont>("font");
            _screenShader = Content.Load<Effect>("ScreenShader");

            //Get all wall textures from the textures/walls folder and load them into our list
            foreach (string file in Directory.GetFiles("Content/textures/walls"))
            {
                wallTextures.Add(Content.Load<Texture2D>("textures/walls/" + Path.GetFileNameWithoutExtension(file)));
            }

            map.SetMapWallTextures(wallTextures);
            map.GenerateMap();


            DrawScreenTexture();
        }

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

                if (keyboardState.IsKeyDown(Keys.F11))
                {
                    _graphics.ToggleFullScreen();
                    DrawScreenTexture();
                }

                lastPressCount = keyboardState.GetPressedKeyCount();
            }




            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            DrawGame(gameTime);

            if (DrawDebug)
            {
                shaderDebug.Draw();
                DrawDebugInfo(gameTime);
            }
        }

        /// <summary>
        /// Sets the screenTexture to the current viewport size.
        /// Should be called any time the viewport size changes.
        /// </summary>
        private void DrawScreenTexture()
        {
            int width = GraphicsDevice.Viewport.Width;
            int height = GraphicsDevice.Viewport.Height;
            screenTexture = new Texture2D(GraphicsDevice, width, height);

            Color[] data = new Color[width * height];
            Array.Fill(data, Color.Orange);
            screenTexture.SetData(data);
        }

        /// <summary>
        /// Draws the game using the screen shader.
        /// </summary>
        /// <param name="gameTime"></param>
        public void DrawGame(GameTime gameTime)
        {
            Texture2D mapTex = map.GetMapTexture(GraphicsDevice, 1, false);
            Vector2 screenSize = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            _screenShader.Parameters["ScreenSize"]?.SetValue(screenSize);
            _screenShader.Parameters["MapTexture"]?.SetValue(mapTex);
            _screenShader.Parameters["WallTexture"]?.SetValue(map.GenerateMapAtlasTexture(GraphicsDevice));
            _screenShader.Parameters["MapSize"]?.SetValue(new Vector2(map.Width, map.Height));
            _screenShader.Parameters["PlayerPosition"]?.SetValue(new Vector2(player.X, player.Y));
            _screenShader.Parameters["PlayerRotation"]?.SetValue(player.Rotation);
            _screenShader.Parameters["MapSamplePoint"]?.SetValue(MapSamplePoint);
            _screenShader.Parameters["WallTexturesCount"]?.SetValue(wallTextures.Count);

            _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, _screenShader);

            //Draw sprite to cover the whole screen for shader to apply properly
            _spriteBatch.Draw(screenTexture, Vector2.Zero, Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        public void DrawDebugInfo(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _spriteBatch.Begin();

            var fps = string.Format("FPS: {0}", _frameCounter.AverageFramesPerSecond);

            //_spriteBatch.Draw(map.GetMapTexture(GraphicsDevice, 10), new Vector2(0, 0), Color.White);

            //_spriteBatch.Draw(map.GenerateMapAtlasTexture(GraphicsDevice), new Vector2(100, 100), Color.White);

            _spriteBatch.DrawString(_spriteFont, fps, new Vector2(1, 1), Color.White);

            _spriteBatch.End();

            _frameCounter.Update(deltaTime);
        }
    }
}
