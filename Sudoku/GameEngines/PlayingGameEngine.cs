using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sudoku.GameLogic;
using SudokuGraphics;

namespace Sudoku.GameEngines
{
    public class PlayingGameEngine : GameEngine
    {

        public PlayingGameEngine(int difficulty)
        {
            //TODO: Initialize the board based on the difficulty level
            var board = SudokuGenerator.NewStartingBoard(difficulty);
        }

        public override void SetScreenSize(int screenWidth, int screenHeight)
        {
            throw new NotImplementedException();
        }

        public override void Draw(GraphicsEngine _graphicsEngine)
        {
            throw new NotImplementedException();
        }

        public override void Update(ref GameStateData _gameStateData)
        {
            throw new NotImplementedException();
        }
    }
}