using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SudokuGraphics;

namespace Sudoku.GameEngines
{
    public class NewGameEngine : GameEngine
    {
        private const int FirstDifficulty = 1;
        private const int LastDifficulty = 5;
        private const int DifficultyOptionHeight = 58;
        private const int DifficultyOptionSpacing = 8;
        private static readonly string[] DifficultyLabels =
        {
            "Beginner",
            "intermediate",
            "Hard",
            "Very Hard",
            "Extreme"
        };
        private MouseState _previousMouseState;
        private KeyboardState _previousKeyboardState;

        public override void Draw(GraphicsEngine _graphicsEngine)
        {
            int centerX = _graphicsEngine.ScreenSize.X / 2;
            int titleY = 70;
            int optionsY = 190;

            DrawCentered(_graphicsEngine, "Sudoku", titleY, Color.Tomato);
            DrawCentered(_graphicsEngine, "Choose difficulty", 135, Color.White);

            for (int difficulty = FirstDifficulty; difficulty <= LastDifficulty; difficulty++)
            {
                Rectangle option = GetDifficultyRectangle(centerX, optionsY, difficulty);
                Color optionColor = difficulty == _selectedDifficulty ? Color.Tomato : Color.DimGray;
                _graphicsEngine.FillRectangle(option, optionColor);
                DrawCentered(_graphicsEngine, DifficultyLabels[difficulty - FirstDifficulty], option.Y + 4, Color.White);
            }

            Rectangle newGameButton = GetNewGameRectangle(centerX, optionsY);
            _graphicsEngine.FillRectangle(newGameButton, Color.ForestGreen);
            DrawCentered(_graphicsEngine, "New Game", newGameButton.Y + 4, Color.White);
        }

        public override void Update(GameTime gameTime, ref GameStateData _gameStateData)
        {
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();
            int centerX = 300;
            int optionsY = 190;

            if (IsNewlyPressed(mouseState.LeftButton, _previousMouseState.LeftButton))
            {
                for (int difficulty = FirstDifficulty; difficulty <= LastDifficulty; difficulty++)
                {
                    if (GetDifficultyRectangle(centerX, optionsY, difficulty).Contains(mouseState.Position))
                    {
                        _selectedDifficulty = difficulty;
                        break;
                    }
                }

                if (GetNewGameRectangle(centerX, optionsY).Contains(mouseState.Position))
                {
                    StartGame(ref _gameStateData);
                }
            }

            if (IsNewlyPressed(keyboardState, Keys.Up))
            {
                _selectedDifficulty = Math.Max(FirstDifficulty, _selectedDifficulty - 1);
            }
            else if (IsNewlyPressed(keyboardState, Keys.Down))
            {
                _selectedDifficulty = Math.Min(LastDifficulty, _selectedDifficulty + 1);
            }
            else if (IsNewlyPressed(keyboardState, Keys.Enter))
            {
                StartGame(ref _gameStateData);
            }

            _previousMouseState = mouseState;
            _previousKeyboardState = keyboardState;
        }

        private int _selectedDifficulty = 3;

        private static Rectangle GetDifficultyRectangle(int centerX, int optionsY, int difficulty)
        {
            int width = 280;
            int y = optionsY + (difficulty - FirstDifficulty) * (DifficultyOptionHeight + DifficultyOptionSpacing);
            return new Rectangle(centerX - width / 2, y, width, DifficultyOptionHeight);
        }

        private static Rectangle GetNewGameRectangle(int centerX, int optionsY)
        {
            int y = optionsY + (LastDifficulty - FirstDifficulty + 1) * (DifficultyOptionHeight + DifficultyOptionSpacing) + 18;
            return new Rectangle(centerX - 180, y, 360, DifficultyOptionHeight);
        }

        private static void DrawCentered(GraphicsEngine graphics, string text, int y, Color color)
        {
            Vector2 size = Art.NewGameFont.MeasureString(text);
            graphics.DrawString(text, new Vector2((graphics.ScreenSize.X - size.X) / 2, y), color);
        }

        private static bool IsNewlyPressed(ButtonState current, ButtonState previous)
        {
            return current == ButtonState.Pressed && previous == ButtonState.Released;
        }

        private bool IsNewlyPressed(KeyboardState current, Keys key)
        {
            return current.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        }

        private void StartGame(ref GameStateData gameStateData)
        {
            gameStateData.CurrentState = GameState.Playing;
            gameStateData.Difficulty = _selectedDifficulty;
        }
    }
}