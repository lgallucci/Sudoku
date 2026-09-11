using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SudokuGraphics
{
    public class RenderContext
    {
        public SpriteBatch SpriteBatch { get; internal set; }
        public Point ScreenSize { get; internal set; }

        public RenderContext()
        {
        }
    }
}
