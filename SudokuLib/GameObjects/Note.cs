namespace SudokuLib.GameObjects
{
    public class Note
    {
        public int Value { get; set; }
        public bool IsNumberHighlighted { get; set; }
        public int Row { get; private set; }
        public int Col { get; private set; }

        public Note(int row, int col)
        {
            Row = row;
            Col = col;
            Value = 0;
            IsNumberHighlighted = false;
        }
    }
}