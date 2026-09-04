using System;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SudokuLib.GameObjects;

namespace SudokuGraphics.DrawObjects
{
    public class PileGraphics : DrawableObject
    {
        internal void DrawPile(SpriteBatch spriteBatch, Pile pile, BoardViewState boardState)
        {
            // Draw an array of numbers 1-9 across the bottom, each on a given-style background with a small remaining-count badge
            var font = Art.CellFont;
            var countFont = Art.NoteFont;
            int slotWidth = boardState.BoardWidth / 9;
            int squareSize = boardState.CellSize;
            int y = boardState.TopLeftY + boardState.BoardHeight + boardState.BorderThickness * 2;

            for (int value = 1; value <= 9; value++)
            {
                int remaining = pile.Values[value];
                bool isUsedUp = remaining <= 0;
                var backgroundColor = isUsedUp ? Color.DarkGray : Color.LightGray;
                var textColor = isUsedUp ? Color.Gray : Color.Black;

                int slotX = boardState.TopLeftX + (value - 1) * slotWidth + (slotWidth - squareSize) / 2;
                spriteBatch.Draw(Art.Pixel, new Rectangle(slotX, y, squareSize, squareSize), backgroundColor);

                var valueString = value.ToString();
                var textSize = font.MeasureString(valueString);
                var countString = $"x{remaining}";
                var countSize = countFont.MeasureString(countString);

                // Center the number and count together, with the smaller count badge to the number's right
                var valuePosition = new Vector2(slotX + (squareSize - textSize.X) / 2, y + (squareSize - textSize.Y) / 2);
                var countPosition = new Vector2(slotX + squareSize - countSize.X, y + (squareSize - countSize.Y));

                spriteBatch.DrawString(font, valueString, valuePosition, textColor);
                spriteBatch.DrawString(countFont, countString, countPosition, textColor);
            }
        }
    }
}