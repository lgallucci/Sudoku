namespace SudokuLib.GameObjects
{
    public class Cell
    {
        public int Value { get; set; }
        public bool IsGiven { get; set; }
        public bool IsSelected { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsNumberHighlighted { get; set; }
        public int Row { get; private set; }
        public int Col { get; private set; }

        public Note[,] Notes { get; set; } = new Note[3, 3];

        public Cell(int row, int col)
        {
            Value = 0;
            IsGiven = false;
            IsSelected = false;
            IsHighlighted = false;
            IsNumberHighlighted = false;
            Row = row;
            Col = col;

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Notes[x, y] = new Note(x, y);
                }
            }
        }

        public void ClearNotes()
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    Notes[row, col].Value = 0;
                    Notes[row, col].IsNumberHighlighted = false;
                }
            }
        }

    }
}