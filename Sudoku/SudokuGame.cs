using System.Collections.Generic;
using Sudoku.GameLogic;
using Microsoft.Xna.Framework;
using SudokuGraphics;
using Sudoku.GameEngines;

namespace Sudoku
{
    public enum GameState
    {
        NewGame = 0,
        Playing = 1,
    }

    public class GameStateData
    {
        public GameState CurrentState { get; set; }
        public int Difficulty { get; set; }
    }

    public class SudokuGame : Game
    {

        private GraphicsEngine _graphicsEngine;
        private GameEngine _gameEngine;
        private GameStateData _gameStateData;
        int _screenWidth = 600;
        int _screenHeight = 800;
        
        public SudokuGame()
        {
            _graphicsEngine = new GraphicsEngine(this);

            Content.RootDirectory = "Content";

            //this.IsFixedTimeStep = false;
            //this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 120d); //60);
            this.IsMouseVisible = true;
            Window.AllowUserResizing = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        private void SetWindowSize()
        {
            _gameEngine.SetScreenSize(_screenWidth, _screenHeight);

            _graphicsEngine.SetScreenSize(_screenWidth, _screenHeight);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _gameEngine = new NewGameEngine();
            SetWindowSize();
            _graphicsEngine.Load(GraphicsDevice, Content);
        }
        
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            _graphicsEngine.Unload();
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                GameState? _previousState =  _gameStateData?.CurrentState;
                _gameEngine.Update(ref _gameStateData);

                if (_gameStateData?.CurrentState != _previousState)
                {
                    if (_gameStateData?.CurrentState == GameState.NewGame)
                    {
                        _gameEngine = new NewGameEngine();
                    } 
                    else if (_gameStateData?.CurrentState == GameState.Playing)
                    {
                        _gameEngine = new PlayingGameEngine(_gameStateData.Difficulty);
                    }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_graphicsEngine.BaseColor);

            _graphicsEngine.BeginSprites();

            _gameEngine.Draw(_graphicsEngine);

            _graphicsEngine.EndSprites();

            base.Draw(gameTime);
        }
    }

}