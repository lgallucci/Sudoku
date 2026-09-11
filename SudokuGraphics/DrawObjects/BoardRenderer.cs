using System;
using FontStashSharp;
using Microsoft.Xna.Framework;
using SudokuLib.GameObjects;

namespace SudokuGraphics.DrawObjects
{
    public class BoardRenderer
    {
        private readonly BoardGraphics _boardGraphics = new BoardGraphics();
        private readonly CellGraphics _cellGraphics = new CellGraphics();
        private readonly NoteGraphics _noteGraphics = new NoteGraphics();
        private readonly PileGraphics _pileGraphics = new PileGraphics();

        public void RenderBoard(Board board, BoardViewState boardState, RenderContext context, TimeSpan elapsedTime)
        {
            BindContext(context);

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    var cell = board.GetCell(row, col);
                    // Render the cell based on its properties
                    _cellGraphics.DrawCell(context.SpriteBatch, cell, boardState);

                    // Render the notes for the cell
                    for (int noteRow = 0; noteRow < 3; noteRow++)
                    {
                        for (int noteCol = 0; noteCol < 3; noteCol++)
                        {
                            var note = cell.Notes[noteRow, noteCol];
                            _noteGraphics.DrawNote(context.SpriteBatch, cell, note, boardState);
                        }
                    }
                }
            }

            _boardGraphics.DrawBorders(context.SpriteBatch, boardState);
            _pileGraphics.DrawPile(context.SpriteBatch, board.Pile, boardState);

            if (boardState.IsSolved)
            {
                var winText = $"You win! Time: {elapsedTime:mm\\:ss}";
                var fontSize = Art.NewGameFont.MeasureString(winText);
                const int horizontalPadding = 24;
                const int verticalPadding = 16;
                var box = new Rectangle(
                    (int)((context.ScreenSize.X - fontSize.X) / 2) - horizontalPadding,
                    (int)((context.ScreenSize.Y - fontSize.Y) / 2) - verticalPadding,
                    (int)fontSize.X + horizontalPadding * 2,
                    (int)fontSize.Y + verticalPadding * 2);
                var textPosition = new Vector2(
                    box.X + horizontalPadding,
                    box.Y + verticalPadding);

                context.SpriteBatch.Draw(Art.Pixel, box, Theme.ButtonPrimary);
                context.SpriteBatch.DrawString(Art.NewGameFont, winText, textPosition, Theme.TextPrimary);
            }
        }

        private void BindContext(RenderContext context)
        {
            _boardGraphics.Bind(context);
            _cellGraphics.Bind(context);
            _noteGraphics.Bind(context);
            _pileGraphics.Bind(context);
        }
    }
}