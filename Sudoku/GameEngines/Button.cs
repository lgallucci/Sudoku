using System;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SudokuGraphics;

namespace Sudoku.GameEngines
{
    public class Button
    {
        private readonly Func<SpriteFontBase> _fontProvider;

        public Rectangle Bounds { get; }
        public string Text { get; }
        public Color Color { get; set; }

        public Button(Rectangle bounds, string text, Color color, Func<SpriteFontBase> fontProvider)
        {
            Bounds = bounds;
            Text = text;
            Color = color;
            _fontProvider = fontProvider;
        }

        public bool Contains(Point position)
        {
            return Bounds.Contains(position);
        }

        public void Draw(GraphicsEngine graphicsEngine)
        {
            graphicsEngine.FillRectangle(Bounds, Color);

            SpriteFontBase font = _fontProvider();
            Vector2 textSize = font.MeasureString(Text);
            Vector2 textPosition = new Vector2(
                Bounds.X + (Bounds.Width - textSize.X) / 2,
                Bounds.Y + (Bounds.Height - textSize.Y) / 2);
            graphicsEngine.SpriteBatch.DrawString(font, Text, textPosition, Theme.TextPrimary);
        }

        public void UpdateCursor(Point position)
        {
            Mouse.SetCursor(Contains(position) ? MouseCursor.Hand : MouseCursor.Arrow);
        }
    }
}