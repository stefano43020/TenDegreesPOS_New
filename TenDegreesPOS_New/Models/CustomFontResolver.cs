using PdfSharp.Drawing;
using PdfSharp.Fonts;
using System.IO;

namespace TenDegreesPOS_New
{
    public class CustomFontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName)
        {
            try
            {
                // Try to find the font in system fonts folder
                string fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                string[] fontFiles = Directory.GetFiles(fontsFolder, "*.ttf");

                foreach (string fontFile in fontFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(fontFile);
                    if (fileName.Equals(faceName, StringComparison.OrdinalIgnoreCase))
                    {
                        return File.ReadAllBytes(fontFile);
                    }
                }

                // Fallback: try Arial
                string arialPath = Path.Combine(fontsFolder, "arial.ttf");
                if (File.Exists(arialPath))
                {
                    return File.ReadAllBytes(arialPath);
                }

                // Fallback: try Calibri
                string calibriPath = Path.Combine(fontsFolder, "calibri.ttf");
                if (File.Exists(calibriPath))
                {
                    return File.ReadAllBytes(calibriPath);
                }
            }
            catch { }

            return null;
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool bold, bool italic)
        {
            // Return default font info
            return new FontResolverInfo("Arial", bold, italic);
        }
    }
}