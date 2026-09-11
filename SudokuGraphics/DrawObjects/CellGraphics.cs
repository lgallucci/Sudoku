using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SudokuLib.GameObjects;
using System;

namespace SudokuGraphics.DrawObjects
{
    public class CellGraphics : DrawableObject
    {
        internal void DrawCell(SpriteBatch spriteBatch, Cell cell, BoardViewState boardState)
        {
            // Draw the cell background
            var cellColor = GetCellColor(cell);
            var cellX = boardState.GetCellX(cell.Col);
            var cellY = boardState.GetCellY(cell.Row);
            spriteBatch.Draw(Art.Pixel, new Rectangle(cellX, cellY, boardState.CellSize, boardState.CellSize), cellColor);

            // Draw the cell value if it exists
            if (cell.Value > 0)
            {
                var valueString = cell.Value.ToString();
                var font = Art.CellFont;
                var textSize = font.MeasureString(valueString);
                var position = new Vector2(cellX + (boardState.CellSize - textSize.X) / 2,
                cellY + (boardState.CellSize - textSize.Y) / 2);
                spriteBatch.DrawString(font, valueString, position, GetFontColor(cell));
            }
        }

        private Color GetCellColor(Cell cell)
        {
             if (cell.IsGiven)
             {
                if (cell.IsSelected)
                    return Theme.TileGivenSelected;
                else if (cell.IsHighlighted)
                    return Theme.TileGivenHighlighted;
                else
                    return Theme.TileGiven;
             }
            else if (cell.IsSelected)
                return Theme.TileEmptySelected;
            else if (cell.IsHighlighted)
                return Theme.TileEmptyHighlighted;
            else
                return Theme.TileEmpty;
        }

        private Color GetFontColor(Cell cell)
        {
            if (cell.IsNumberHighlighted)
                return Theme.HighlightColor;
            else
                if (cell.IsGiven)
                    return Theme.NumberGiven;
                else
                    return Theme.NumberInput;
        }
    }
}