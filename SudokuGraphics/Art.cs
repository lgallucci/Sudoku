using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SudokuGraphics
{
    public class Art
    {
        public static DynamicSpriteFont NoteFont { get; private set; }
        public static DynamicSpriteFont CellFont { get; private set; }
        public static DynamicSpriteFont NewGameFont { get; private set; }
        internal static Texture2D Pixel { get; private set; }

        internal static void Load(ContentManager content, GraphicsDevice device)
        {
            // Load font system
            var robotoFont = FontSystemFactory.Create(device, 1024, 1024);
            robotoFont.AddFont(File.ReadAllBytes("Content/fonts/roboto.ttf"));

            NoteFont = robotoFont.GetFont(16);
            CellFont = robotoFont.GetFont(44);
            NewGameFont = robotoFont.GetFont(48);

            Pixel = new Texture2D(device, 1, 1);
            Pixel.SetData(new[] { Microsoft.Xna.Framework.Color.White });
        }
    }
}