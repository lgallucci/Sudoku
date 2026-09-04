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
            (_removedValues, var startingBoard, _solution) = SudokuGenerator.NewStartingBoard(difficulty);

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
            throw new NotImplementedException();
        }
    }
}