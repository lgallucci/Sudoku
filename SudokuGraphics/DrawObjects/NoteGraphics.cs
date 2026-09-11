using System;
using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SudokuLib.GameObjects;

namespace SudokuGraphics.DrawObjects
{
    public class NoteGraphics : DrawableObject
    {
        internal void DrawNote(SpriteBatch spriteBatch, Cell cell, Note note, BoardViewState boardState)
        {
            if (note.Value > 0)
            {
                var noteString = note.Value.ToString();
                var font = Art.NoteFont;
                var textSize = font.MeasureString(noteString);

                // Calculate the position for the note within the cell
                int noteRow = note.Row;
                int noteCol = note.Col;
                float cellSize = boardState.CellSize;
                float noteSize = cellSize / 3; // Assuming 3x3 notes in a cell
                var position = new Vector2(
                    boardState.GetCellX(cell.Col) + noteCol * noteSize + (noteSize - textSize.X) / 2,
                    boardState.GetCellY(cell.Row) + noteRow * noteSize + (noteSize - textSize.Y) / 2
                );

                var layout = new RichTextLayout()
                {
                    Font = font,
                    Text = note.IsNumberInvalid ? "/ts" + noteString + "/td" : noteString,
                };
                layout.Draw(spriteBatch, position, GetFontColor(cell, note));
            }
        }

        private Color GetFontColor(Cell cell, Note note)
        {
            if (note.IsNumberInvalid)
                return Theme.InvalidColor;
            if (note.IsNumberHighlighted)
                return Theme.HighlightColor;
            else
                return Theme.NoteNormal;
        }
    }
}