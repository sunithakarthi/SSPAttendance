using DevExpress.Drawing;
using System.Reflection;

namespace DevExpressProjectTemplate.Services {
    public static class FontLoader {
        public async static Task LoadFonts(HttpClient httpClient, List<string> fontNames) {
            foreach(var fontName in fontNames) {
                var fontBytes = await httpClient.GetByteArrayAsync($"fonts/{fontName}");
                DXFontRepository.Instance.AddFont(fontBytes);
            }
        }
    }
}
