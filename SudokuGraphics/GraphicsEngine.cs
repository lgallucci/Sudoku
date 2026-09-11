using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SudokuGraphics
{
    public class GraphicsEngine
    {
        public Point ScreenSize { get; set; }
        public GraphicsDeviceManager GraphicsDeviceManager { get; set; }
        public GraphicsDevice Device { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }
        public RenderContext Context { get; private set; }
        public Color BaseColor { get; set; } = Theme.Background;
        private RenderTarget2D _sceneRenderTarget;

        public GraphicsEngine(Game game)
        {
            GraphicsDeviceManager = new GraphicsDeviceManager(game);

            GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
            GraphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            
            Context = new RenderContext();
        }

        public void Load(GraphicsDevice device, ContentManager content)
        {
            Device = device;
            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(device);
            Context.SpriteBatch = SpriteBatch;

            _sceneRenderTarget = CreateSceneRenderTarget();

            Art.Load(content, device);
        }

        public void Unload()
        {
            _sceneRenderTarget?.Dispose();
        }

        private RenderTarget2D CreateSceneRenderTarget()
        {
            return new RenderTarget2D(Device, ScreenSize.X, ScreenSize.Y, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        public void SetScreenSize(int screenWidth, int screenHeight)
        {
            ScreenSize = new Point(screenWidth, screenHeight);
            Context.ScreenSize = ScreenSize;

            GraphicsDeviceManager.PreferredBackBufferWidth = screenWidth;
            GraphicsDeviceManager.PreferredBackBufferHeight = screenHeight;

            GraphicsDeviceManager.ApplyChanges();

            if (Device != null && _sceneRenderTarget != null)
            {
                _sceneRenderTarget.Dispose();
                _sceneRenderTarget = CreateSceneRenderTarget();
            }
        }
        
        public void BeginSprites()
        {
            Device.SetRenderTarget(_sceneRenderTarget);
            Device.Clear(BaseColor);
            SpriteBatch.Begin();
        }

        public void EndSprites()
        {
            SpriteBatch.End();

            Device.SetRenderTarget(null);
            Device.Clear(Color.Transparent);

            SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            SpriteBatch.Draw(_sceneRenderTarget, Vector2.Zero, Theme.RenderTargetTint);
            SpriteBatch.End();
        }

        //Drawing Methods
        public void DrawString(string text)
        {
            var fontSize = Art.NewGameFont.MeasureString(text);
            SpriteBatch.DrawString(Art.NewGameFont, text, new Vector2((ScreenSize.X / 2) - fontSize.X / 2, (ScreenSize.Y / 2) - fontSize.Y / 2), Theme.ScreenAccent);
        }

        public void DrawString(string text, Vector2 position, Color color)
        {
            SpriteBatch.DrawString(Art.NewGameFont, text, position, color);
        }

        public void FillRectangle(Rectangle rectangle, Color color)
        {
            SpriteBatch.Draw(Art.Pixel, rectangle, color);
        }
    }
}