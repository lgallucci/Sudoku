using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SudokuGraphics.DrawObjects
{
    public class BoardGraphics : DrawableObject
    {
        internal void DrawBorders(SpriteBatch spriteBatch, BoardViewState boardState)
        {
            // Draw the borders of the Sudoku board
            var borderColor = boardState.BorderColor;
            var borderThickness = boardState.BorderThickness;
            var boardWidth = boardState.BoardWidth;
            var boardHeight = boardState.BoardHeight;
            var innerWidth = boardWidth - 2 * borderThickness;
            var innerHeight = boardHeight - 2 * borderThickness;

            // Draw outer border
            spriteBatch.Draw(Art.Pixel, new Rectangle(boardState.TopLeftX, boardState.TopLeftY, boardWidth, borderThickness), borderColor); // Top
            spriteBatch.Draw(Art.Pixel, new Rectangle(boardState.TopLeftX, boardState.TopLeftY + boardHeight - borderThickness, boardWidth, borderThickness), borderColor); // Bottom
            spriteBatch.Draw(Art.Pixel, new Rectangle(boardState.TopLeftX, boardState.TopLeftY, borderThickness, boardHeight), borderColor); // Left
            spriteBatch.Draw(Art.Pixel, new Rectangle(boardState.TopLeftX + boardWidth - borderThickness, boardState.TopLeftY, borderThickness, boardHeight), borderColor); // Right

            // Draw inner grid lines
            for (int i = 1; i < 9; i++)
            {
                int thickness = (i % 3 == 0) ? borderThickness : 1; // Thicker lines for every third line

                // Horizontal lines
                spriteBatch.Draw(Art.Pixel, new Rectangle(boardState.TopLeftX + borderThickness, boardState.GetCellY(i) - thickness, innerWidth, thickness), borderColor);

                // Vertical lines
                spriteBatch.Draw(Art.Pixel, new Rectangle(boardState.GetCellX(i) - thickness, boardState.TopLeftY + borderThickness, thickness, innerHeight), borderColor);
            }
        }
    }
}