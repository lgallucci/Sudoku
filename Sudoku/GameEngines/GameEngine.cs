using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sudoku.GameEngines
{
    public abstract class GameEngine
    {
        public abstract void SetScreenSize(int screenWidth, int screenHeight);
        public abstract void Draw(SudokuGraphics.GraphicsEngine _graphicsEngine);
        public abstract void Update(ref GameStateData _gameStateData);
    }
}