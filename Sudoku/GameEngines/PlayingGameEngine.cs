using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SudokuLib.GameLogic;
using SudokuGraphics;
using SudokuLib.GameObjects;

namespace Sudoku.GameEngines
{
    public class PlayingGameEngine : GameEngine
    {
        private readonly BoardViewState _view;
        private readonly PlayingGameUi _ui;
        private readonly Board _board;
        private readonly int[,] _solution;
        private readonly string _hardestAlgorithm;
        private readonly Stopwatch _gameTimer;
        private MouseState _previousMouseState;
        private KeyboardState _previousKeyboardState;
        private bool _showHint;
        
        public PlayingGameEngine(int difficulty)
        {
            int tier = Math.Clamp(difficulty, 1, 5);
            string puzzlePath = Path.Combine(AppContext.BaseDirectory, PuzzleFileFormat.GetTierFileName(tier));
            using var reader = new PuzzleTierReader(puzzlePath);
            PuzzleRecord puzzle = reader.GetRandom(new Random());

            int[,] startingBoard = ParseBoard(puzzle.Puzzle);
            _solution = ParseBoard(puzzle.Solution);
            _hardestAlgorithm = puzzle.HardestAlgorithm;

            _board = new Board(startingBoard);

            _view = new BoardViewState();
            _ui = new PlayingGameUi();
            _gameTimer = Stopwatch.StartNew();

            _previousMouseState = Mouse.GetState();
            _previousKeyboardState = Keyboard.GetState();
        }

        private static int[,] ParseBoard(string serializedBoard)
        {
            int[,] board = new int[9, 9];
            for (int index = 0; index < serializedBoard.Length; index++)
            {
                char value = serializedBoard[index];
                board[index / 9, index % 9] = value == '.' ? 0 : value - '0';
            }

            return board;
        }

        public override void Draw(GraphicsEngine graphicsEngine)
        {
            _ui.Draw(graphicsEngine, _board, _view, _gameTimer.Elapsed, _hardestAlgorithm, _showHint);
        }

        public override void Update(GameTime gameTime, ref GameStateData _gameStateData)
        {
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();

            _ui.UpdateCursor(mouseState.Position);

            if (IsNewlyPressed(mouseState.LeftButton, _previousMouseState.LeftButton))
            {
                HandleMouseClick(mouseState.Position, ref _gameStateData);
            }

            if (_gameStateData.CurrentState != GameState.Playing)
            {
                _previousMouseState = mouseState;
                _previousKeyboardState = keyboardState;
                return;
            }

            if (_ui.IsConfirmationOpen)
            {
                _previousMouseState = mouseState;
                _previousKeyboardState = keyboardState;
                return;
            }

            if (!_view.IsSolved)
            {
                if (IsNewlyPressed(mouseState.LeftButton, _previousMouseState.LeftButton))
                {
                    HandleGameplayMouseClick(mouseState.Position);
                }

                HandleArrowKeys(keyboardState);
                HandleKeyboardInput(keyboardState);
                CheckForSolved();
            }
            _previousMouseState = mouseState;
            _previousKeyboardState = keyboardState;
        }

        private void HandleMouseClick(Point position, ref GameStateData gameStateData)
        {
            switch (_ui.HandleClick(position))
            {
                case PlayingUiAction.ShowHint:
                    _showHint = true;
                    return;
                case PlayingUiAction.FillNotes:
                    FillAllNotes();
                    return;
                case PlayingUiAction.StartNewGame:
                    gameStateData.CurrentState = GameState.NewGame;
                    return;
            }

            if (!_ui.IsConfirmationOpen && _view.IsSolved)
            {
                gameStateData.CurrentState = GameState.NewGame;
            }
        }

        private void HandleGameplayMouseClick(Point position)
        {
            if (!TrySelectPileValue(position))
            {
                SelectCellAt(position.X, position.Y);
            }
        }

        private bool IsNewlyPressed(ButtonState current, ButtonState previous)
        {
            return current == ButtonState.Pressed && previous == ButtonState.Released;
        }

        private void FillAllNotes()
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    var cell = _board.GetCell(row, col);
                    if (cell.Value != 0)
                    {
                        continue;
                    }

                    for (int noteRow = 0; noteRow < 3; noteRow++)
                    {
                        for (int noteCol = 0; noteCol < 3; noteCol++)
                        {
                            cell.Notes[noteRow, noteCol].Value = 0;
                            cell.Notes[noteRow, noteCol].IsNumberHighlighted = false;
                            cell.Notes[noteRow, noteCol].IsNumberInvalid = false;
                        }
                    }

                    for (int value = 1; value <= 9; value++)
                    {
                        if (IsValueAllowed(row, col, value))
                        {
                            var note = cell.Notes[(value - 1) / 3, (value - 1) % 3];
                            note.Value = value;
                            note.IsNumberHighlighted = value == _view.SelectedValue;
                        }
                    }
                }
            }
        }

        private bool IsValueAllowed(int row, int col, int value)
        {
            for (int index = 0; index < 9; index++)
            {
                if (index != col && _board.GetCell(row, index).Value == value)
                {
                    return false;
                }

                if (index != row && _board.GetCell(index, col).Value == value)
                {
                    return false;
                }
            }

            int startRow = (row / 3) * 3;
            int startCol = (col / 3) * 3;
            for (int r = startRow; r < startRow + 3; r++)
            {
                for (int c = startCol; c < startCol + 3; c++)
                {
                    if ((r != row || c != col) && _board.GetCell(r, c).Value == value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void SelectCellAt(int x, int y)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    int cellX = _view.GetCellX(col);
                    int cellY = _view.GetCellY(row);
                    if (x >= cellX && x < cellX + _view.CellSize && y >= cellY && y < cellY + _view.CellSize)
                    {
                        SelectCell(_board.GetCell(row, col));
                        return;
                    }
                }
            }
        }

        private bool TrySelectPileValue(Point position)
        {
            int slotWidth = _view.BoardWidth / 9;
            int pileY = _view.TopLeftY + _view.BoardHeight + _view.BorderThickness * 2;
            if (position.Y < pileY || position.Y >= pileY + _view.CellSize)
            {
                return false;
            }

            int slot = (position.X - _view.TopLeftX) / slotWidth;
            if (slot < 0 || slot >= 9)
            {
                return false;
            }

            int slotX = _view.TopLeftX + slot * slotWidth + (slotWidth - _view.CellSize) / 2;
            if (position.X < slotX || position.X >= slotX + _view.CellSize)
            {
                return false;
            }

            int value = slot + 1;
            if (_board.Pile.Values[value] <= 0)
            {
                return true;
            }

            SelectValue(value);
            return true;
        }

        private void SelectCell(Cell cell)
        {
            if (cell.Value == 0)
            {
                SetSelectedCell(cell);
                return;
            }

            ClearHighlights();
            SetSelectedCell(cell);
            _view.SelectedValue = cell.Value;

            HighlightRowColumnAndBox(cell);
            HighlightMatchingNumbers(cell.Value);
        }

        private void SetSelectedCell(Cell cell)
        {
            if (_view.SelectedCell != null)
            {
                _view.SelectedCell.IsSelected = false;
            }
            cell.IsSelected = true;
            _view.SelectedCell = cell;
        }

        private void SelectValue(int value)
        {
            ClearHighlights();

            if (_view.SelectedCell != null)
            {
                _view.SelectedCell.IsSelected = false;
                _view.SelectedCell = null;
            }

            _view.SelectedValue = value;
            HighlightMatchingNumbers(value);
        }

        private void ClearHighlights()
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    var cell = _board.GetCell(row, col);
                    cell.IsHighlighted = false;
                    cell.IsNumberHighlighted = false;

                    foreach (var note in cell.Notes)
                    {
                        note.IsNumberHighlighted = false;
                    }
                }
            }
        }

        private void ClearNotesRowColumnAndBox(Cell selectedCell)
        {
            ExecuteOnRowAndColumn(selectedCell, (row, col) => 
            { 
                ClearNote(_board.GetCell(row, col), selectedCell.Value);
            });
        }

        private void HighlightRowColumnAndBox(Cell selectedCell)
        {
            ExecuteOnRowAndColumn(selectedCell, (row, col) => 
            { 
                _board.GetCell(row, col).IsHighlighted = true;
            });            
        }

        private void ExecuteOnRowAndColumn(Cell selectedCell, Action<int, int> action)
        {            
            int startRow = (selectedCell.Row / 3) * 3;
            int startCol = (selectedCell.Col / 3) * 3;
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    bool inRow = row == selectedCell.Row;
                    bool inCol = col == selectedCell.Col;
                    bool inBox = row >= startRow && row < startRow + 3 && col >= startCol && col < startCol + 3;

                    if (inRow || inCol || inBox)
                    {
                        action(row, col);
                    }
                }
            }
        }

        private void HighlightMatchingNumbers(int value)
        {
            if (value <= 0)
            {
                return;
            }

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    var cell = _board.GetCell(row, col);
                    if (cell.Value == value)
                    {
                        cell.IsNumberHighlighted = true;
                    }

                    foreach (var note in cell.Notes)
                    {
                        if (note.Value == value)
                        {
                            note.IsNumberHighlighted = true;
                        }
                    }
                }
            }
        }

        private void HandleArrowKeys(KeyboardState keyboardState)
        {
            int rowDelta = 0;
            int colDelta = 0;

            if (IsKeyNewlyPressed(keyboardState, Keys.Up)) rowDelta = -1;
            else if (IsKeyNewlyPressed(keyboardState, Keys.Down)) rowDelta = 1;
            else if (IsKeyNewlyPressed(keyboardState, Keys.Left)) colDelta = -1;
            else if (IsKeyNewlyPressed(keyboardState, Keys.Right)) colDelta = 1;

            if (rowDelta == 0 && colDelta == 0)
            {
                return;
            }

            var current = _view.SelectedCell;
            int row = Math.Clamp((current?.Row ?? 0) + rowDelta, 0, 8);
            int col = Math.Clamp((current?.Col ?? 0) + colDelta, 0, 8);
            SelectCell(_board.GetCell(row, col));
        }

        private bool IsKeyNewlyPressed(KeyboardState keyboardState, Keys key)
        {
            return keyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        }

        private void HandleKeyboardInput(KeyboardState keyboardState)
        {
            var selectedCell = _view.SelectedCell;
            if (selectedCell == null || selectedCell.IsGiven)
            {
                return;
            }

            bool shiftHeld = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);

            foreach (var key in keyboardState.GetPressedKeys())
            {
                if (_previousKeyboardState.IsKeyDown(key))
                {
                    continue; // only react to newly pressed keys, not held-down keys
                }

                int value = GetNumberFromKey(key);
                if (value > 0)
                {
                    if (shiftHeld)
                    {
                        ToggleNote(selectedCell, value);
                    }
                    else if (_board.TrySetCellValue(selectedCell.Row, selectedCell.Col, value))
                    {
                        ClearNotesRowColumnAndBox(selectedCell);
                        SelectCell(selectedCell); // refresh highlights to match the cell's new value
                    }
                    else
                    {
                        _board.SetInvalidCellValue(selectedCell.Row, selectedCell.Col, value);                        
                    }
                }
                else if (key == Keys.Delete || key == Keys.Back)
                {
                    if (_board.TrySetCellValue(selectedCell.Row, selectedCell.Col, 0))
                    {
                        SelectCell(selectedCell);
                    }
                }
            }
        }

        private static int GetNumberFromKey(Keys key)
        {
            if (key >= Keys.D1 && key <= Keys.D9)
            {
                return key - Keys.D1 + 1;
            }
            if (key >= Keys.NumPad1 && key <= Keys.NumPad9)
            {
                return key - Keys.NumPad1 + 1;
            }
            return 0;
        }

        private void ToggleNote(Cell cell, int value)
        {
            var note = cell.Notes[(value - 1) / 3, (value - 1) % 3];
            note.Value = note.Value == value ? 0 : value;
            note.IsNumberHighlighted = note.Value == _view.SelectedValue;

            ExecuteOnRowAndColumn(cell, (row, col) =>
            {
                var matchingCell = _board.GetCell(row, col);
                if (matchingCell.Value == value)
                {
                    note.IsNumberInvalid = true;
                }
            });

        }

        private static void ClearNote(Cell cell, int value)
        {
            var note = cell.Notes[(value - 1) / 3, (value - 1) % 3];
            note.IsNumberInvalid = false;
            note.Value = 0;
            note.IsNumberHighlighted = false;
        }

        private void CheckForSolved()
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (_board.GetCell(row, col).Value != _solution[row, col])
                    {
                        return;
                    }
                }
            }
            _view.IsSolved = true;
            _gameTimer.Stop();
        }
    }
}