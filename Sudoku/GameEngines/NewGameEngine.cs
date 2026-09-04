using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SudokuGraphics;

namespace Sudoku.GameEngines
{
    public class NewGameEngine : GameEngine
    {
        public override void Draw(GraphicsEngine _graphicsEngine)
        {
            _graphicsEngine.DrawString("Sudoku !   Click to play...");
        }

        public override void Update(GameTime gameTime, ref GameStateData _gameStateData)
        {
            var mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                _gameStateData.CurrentState = GameState.Playing;
            }
        }
    }
}