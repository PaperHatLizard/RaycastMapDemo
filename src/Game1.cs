using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RaycastMapDemo
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;

        private RaycastRenderer raycastRenderer;
        private Player player;
        private FrameCounter _frameCounter = new FrameCounter();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _spriteFont = Content.Load<SpriteFont>("font");

            Map map = new Map(10,10);
            player = new Player(2, 7, 0, map);


            raycastRenderer = new RaycastRenderer(
                GraphicsDevice, 
                GraphicsDevice.Viewport.Width, 
                GraphicsDevice.Viewport.Height, 
                map, player);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            player.Update(gameTime);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            raycastRenderer.Draw(GraphicsDevice, gameTime);

            _spriteBatch.Begin();

            _spriteBatch.Draw(raycastRenderer, Vector2.Zero, Color.White);

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _frameCounter.Update(deltaTime);

            var fps = string.Format("FPS: {0}", _frameCounter.AverageFramesPerSecond);

            _spriteBatch.DrawString(_spriteFont, fps, new Vector2(1, 1), Color.Black);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
