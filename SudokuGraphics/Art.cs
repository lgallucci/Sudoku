using System;
using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SudokuGraphics
{
    public class Art
    {
        public static SpriteFontBase NoteFont { get; private set; }
        public static SpriteFontBase CellFont { get; private set; }
        public static SpriteFontBase NewGameFont { get; private set; }
        internal static Texture2D Pixel { get; private set; }

        internal static void Load(ContentManager content, GraphicsDevice device)
        {
            // Load font system
            FontSystem robotoFont = new FontSystem();

            byte[] fontBytes = File.ReadAllBytes(
                Path.Combine(AppContext.BaseDirectory, "Content", "fonts", "roboto.ttf"));
            robotoFont.AddFont(fontBytes);

            NoteFont = robotoFont.GetFont(20);
            CellFont = robotoFont.GetFont(44);
            NewGameFont = robotoFont.GetFont(48);

            Pixel = new Texture2D(device, 1, 1);
            Pixel.SetData(new[] { Microsoft.Xna.Framework.Color.White });
        }
    }
}