using Microsoft.Xna.Framework;
using SudokuLib.GameObjects;

namespace SudokuGraphics
{

    public sealed class BoardViewState
    {
        public int CellSize { get; internal set; } = 60;
        public int TopLeftX { get; internal set; } = 20;
        public int TopLeftY { get; internal set; } = 100;
        public int BorderThickness { get; internal set; } = 4;
        public Color BorderColor { get; internal set; } = Theme.BoardBorder;
        public int SelectedValue { get; set; }
        public Cell SelectedCell { get; set; }
        public bool IsSolved { get; set; }

        public int BoardWidth => 2 * BorderThickness + 9 * CellSize + GetGapOffset(8);
        public int BoardHeight => BoardWidth;

        public int GetCellX(int col) => TopLeftX + BorderThickness + col * CellSize + GetGapOffset(col);
        public int GetCellY(int row) => TopLeftY + BorderThickness + row * CellSize + GetGapOffset(row);

        // Sum of the thickness of the internal grid lines before column/row 'index' (thick every 3rd line, thin otherwise)
        private int GetGapOffset(int index)
        {
            int thickCount = index / 3;
            int thinCount = index - thickCount;
            return thinCount * 1 + thickCount * BorderThickness;
        }
    }
}