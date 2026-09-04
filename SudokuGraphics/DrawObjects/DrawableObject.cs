using System.Drawing;

namespace SudokuGraphics.DrawObjects
{
    public abstract class DrawableObject
    {
        protected RenderContext Context { get; private set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Rectangle Location { get; set; }

        internal void Bind(RenderContext context)
        {
            Context = context;
        }
    }
}