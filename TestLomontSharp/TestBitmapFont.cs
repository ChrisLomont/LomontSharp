using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomont.Graphics;
using Lomont.Graphics.Fonts;
using NUnit.Framework;

namespace TestLomontSharp
{
    public class TestBitmapFont
    {
        [Test]
        public void TestFont()
        {
            var bmp = new SimpleBitmap(1000, 2000);
            bmp.Fill(Color.Black);
            int i = 10, j = 10, lines = 3;
            foreach (var fd in BitmapFont.GetFontDescriptions())
            {
                var font = new BitmapFont(fd.Name);
                bmp.DrawText(i,j,$"{fd.Name}\nABC0123\nabc0123",font);
                j += lines * (fd.Height + fd.Ascender + fd.Descender + 5);
            }
            bmp.Save("BitmapFonts.png");

        }
    }
}
