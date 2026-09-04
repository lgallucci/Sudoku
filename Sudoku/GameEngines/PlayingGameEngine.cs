using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
            //TODO: Implement keyboard input handling (number keys and numapad) for selecting cells and entering values.
            //TODO: Implement keyboard input handling for entering notes in cells. (shift + number)
            //TODO: Implement mouse input handling for selecting cells and interacting with the game.
            //TODO: Implement game logic for checking if the board is solved and updating the game state accordingly.            
        }
    }
}