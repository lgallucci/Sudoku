using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sudoku.GameLogic;
using SudokuGraphics;
using SudokuGraphics.DrawObjects;
using SudokuLib.GameObjects;

namespace Sudoku.GameEngines
{
    public class PlayingGameEngine : GameEngine
    {
        private readonly BoardViewState _view;
        private readonly BoardRenderer _boardRenderer;
        private readonly Board _board;
        private readonly int[,] _solution;
        private readonly List<(int, int, int)> _removedValues;
        private MouseState _previousMouseState;
        private KeyboardState _previousKeyboardState;
        private bool _isSolved;
        
        public PlayingGameEngine(int difficulty)
        {
            int[,] startingBoard;
            if (difficulty == 1)
            {
                (_removedValues, startingBoard, _solution) = SudokuGenerator.NewStartingBoard(36);
            }
            else if (difficulty == 2)
            {
                (_removedValues, startingBoard, _solution) = SudokuGenerator.NewStartingBoard(46);
            }
            else if (difficulty == 3)
            {
                (_removedValues, startingBoard, _solution) = SudokuGenerator.NewStartingBoard(51);
            }
            else
            {
                (_removedValues, startingBoard, _solution) = SudokuGenerator.NewStartingBoard(55);
            }

            _board = new Board(startingBoard);

            _view = new BoardViewState();
            _boardRenderer = new BoardRenderer();
        }

        public override void Draw(GraphicsEngine _graphicsEngine)
        {
            _boardRenderer.RenderBoard(_board, _view, _graphicsEngine.Context);
        }

        public override void Update(GameTime gameTime, ref GameStateData _gameStateData)
        {
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();

            if (!_isSolved)
            {
                if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
                {
                    SelectCellAt(mouseState.X, mouseState.Y);
                }

                HandleArrowKeys(keyboardState);
                HandleKeyboardInput(keyboardState);
                CheckForSolved();
            }

            _previousMouseState = mouseState;
            _previousKeyboardState = keyboardState;
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

        private void SelectCell(Cell cell)
        {
            if (_view.SelectedCell != null)
            {
                _view.SelectedCell.IsSelected = false;
            }
            cell.IsSelected = true;
            _view.SelectedCell = cell;
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
                    else
                    {
                        _board.TrySetCellValue(selectedCell.Row, selectedCell.Col, value);
                    }
                }
                else if (key == Keys.Delete || key == Keys.Back)
                {
                    _board.TrySetCellValue(selectedCell.Row, selectedCell.Col, 0);
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

        private static void ToggleNote(Cell cell, int value)
        {
            var note = cell.Notes[(value - 1) / 3, (value - 1) % 3];
            note.Value = note.Value == value ? 0 : value;
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
            _isSolved = true;
        }
    }
}