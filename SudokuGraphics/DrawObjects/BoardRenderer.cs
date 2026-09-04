using SudokuLib.GameObjects;

namespace SudokuGraphics.DrawObjects
{
    public class BoardRenderer
    {
        private readonly BoardGraphics _boardGraphics = new BoardGraphics();
        private readonly CellGraphics _cellGraphics = new CellGraphics();
        private readonly NoteGraphics _noteGraphics = new NoteGraphics();
        private readonly PileGraphics _pileGraphics = new PileGraphics();

        public void RenderBoard(Board board, BoardViewState boardState, RenderContext context)
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