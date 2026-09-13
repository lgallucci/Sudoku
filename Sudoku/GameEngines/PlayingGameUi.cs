using System;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SudokuGraphics;
using SudokuGraphics.DrawObjects;
using SudokuLib.GameObjects;

namespace Sudoku.GameEngines
{
    public enum PlayingUiAction
    {
        None,
        ShowHint,
        FillNotes,
        StartNewGame
    }

    public sealed class PlayingGameUi
    {
        private readonly BoardRenderer _boardRenderer = new BoardRenderer();
        private readonly Button _hintButton;
        private readonly Button _fillNotesButton;
        private readonly Button _newGameButton;
        private readonly ConfirmationDialog _newGameDialog = new ConfirmationDialog();

        public PlayingGameUi()
        {
            _hintButton = new Button(new Rectangle(450, 20, 125, 38), "Show Hint", Theme.ButtonPrimary, () => Art.NoteFont);
            _fillNotesButton = new Button(new Rectangle(235, 20, 125, 38), "Fill Notes", Theme.ButtonPrimary, () => Art.NoteFont);
            _newGameButton = new Button(new Rectangle(20, 20, 125, 38), "New Game", Theme.ButtonPrimary, () => Art.NoteFont);
        }

        public bool IsConfirmationOpen => _newGameDialog.IsOpen;

        public void Draw(
            GraphicsEngine graphicsEngine,
            Board board,
            BoardViewState view,
            TimeSpan elapsedTime,
            string hardestAlgorithm,
            bool showHint)
        {
            _boardRenderer.RenderBoard(board, view, graphicsEngine.Context, elapsedTime);
            _hintButton.Draw(graphicsEngine);
            _fillNotesButton.Draw(graphicsEngine);
            _newGameButton.Draw(graphicsEngine);

            if (showHint)
            {
                DrawHint(graphicsEngine, hardestAlgorithm);
            }

            if (_newGameDialog.IsOpen)
            {
                _newGameDialog.Draw(graphicsEngine);
            }
        }

        public void UpdateCursor(Point position)
        {
            if (_newGameDialog.IsOpen)
            {
                _newGameDialog.UpdateCursor(position);
                return;
            }

            if (_hintButton.Contains(position))
            {
                _hintButton.UpdateCursor(position);
                return;
            }

            if (_fillNotesButton.Contains(position))
            {
                _fillNotesButton.UpdateCursor(position);
                return;
            }

            _newGameButton.UpdateCursor(position);
        }

        public PlayingUiAction HandleClick(Point position)
        {
            if (_newGameDialog.IsOpen)
            {
                return _newGameDialog.HandleClick(position)
                    ? PlayingUiAction.StartNewGame
                    : PlayingUiAction.None;
            }

            if (_newGameButton.Contains(position))
            {
                _newGameDialog.Open();
            }
            else if (_hintButton.Contains(position))
            {
                return PlayingUiAction.ShowHint;
            }
            else if (_fillNotesButton.Contains(position))
            {
                return PlayingUiAction.FillNotes;
            }

            return PlayingUiAction.None;
        }

        private static void DrawHint(GraphicsEngine graphicsEngine, string hardestAlgorithm)
        {
            string hintText = $"Hint: {hardestAlgorithm}";
            Vector2 textSize = Art.NoteFont.MeasureString(hintText);
            Vector2 hintPosition = new Vector2(
                Math.Max(20, graphicsEngine.ScreenSize.X - textSize.X - 20),
                68);
            graphicsEngine.SpriteBatch.DrawString(Art.NoteFont, hintText, hintPosition, Theme.TextPrimary);
        }
}
}
