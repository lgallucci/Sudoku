using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SudokuGraphics;

namespace Sudoku.GameEngines
{
    public sealed class ConfirmationDialog
    {
        private readonly Rectangle _bounds = new Rectangle(120, 250, 360, 250);
        private readonly Button _yesButton;
        private readonly Button _noButton;

        public bool IsOpen { get; private set; }

        public ConfirmationDialog()
        {
            _yesButton = new Button(new Rectangle(220, 430, 75, 38), "Yes", Theme.ButtonPrimary, () => Art.NoteFont);
            _noButton = new Button(new Rectangle(305, 430, 75, 38), "No", Theme.ButtonSecondary, () => Art.NoteFont);
        }

        public void Open()
        {
            IsOpen = true;
        }

        public bool HandleClick(Point position)
        {
            if (_yesButton.Contains(position))
            {
                IsOpen = false;
                return true;
            }

            if (_noButton.Contains(position))
            {
                IsOpen = false;
            }

            return false;
        }

        public void UpdateCursor(Point position)
        {
            Mouse.SetCursor(_yesButton.Contains(position) || _noButton.Contains(position)
                ? MouseCursor.Hand
                : MouseCursor.Arrow);
        }

        public void Draw(GraphicsEngine graphicsEngine)
        {
            graphicsEngine.FillRectangle(_bounds, Theme.Background);
            const string message = "Start a new game?";
            Vector2 textSize = Art.NoteFont.MeasureString(message);
            graphicsEngine.SpriteBatch.DrawString(
                Art.NoteFont,
                message,
                new Vector2(_bounds.X + (_bounds.Width - textSize.X) / 2, _bounds.Y + 45),
                Theme.TextPrimary);
            _yesButton.Draw(graphicsEngine);
            _noButton.Draw(graphicsEngine);
        }
    }
}
