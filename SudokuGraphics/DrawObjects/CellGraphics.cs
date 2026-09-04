using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SudokuLib.GameObjects;

namespace SudokuGraphics.DrawObjects
{
    public class CellGraphics : DrawableObject
    {
        internal void DrawCell(SpriteBatch spriteBatch, Cell cell, BoardViewState boardState)
        {
            // Draw the cell background
            var cellColor = cell.IsSelected ? Color.LightBlue : Color.White;
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
                spriteBatch.DrawString(font, valueString, position, Color.Black);
            }
        }
    }
}