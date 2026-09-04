using Microsoft.Xna.Framework;

namespace Sudoku.GameEngines
{
    public abstract class GameEngine
    {
        public abstract void Draw(SudokuGraphics.GraphicsEngine _graphicsEngine);
        public abstract void Update(GameTime gameTime, ref GameStateData _gameStateData);
    }
}