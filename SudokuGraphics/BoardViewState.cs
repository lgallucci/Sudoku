using Microsoft.Xna.Framework;
using SudokuLib.GameObjects;

namespace SudokuGraphics
{

    public sealed class BoardViewState
    {
        public int CellSize { get; internal set; } = 50;
        public int SelectedValue { get; internal set; }
        public Cell SelectedCell { get; internal set; }
    }
}