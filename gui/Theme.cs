using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Theme {
    public Color Transparent { get; set; }                      = new Color(0, 0, 0, 0);            // Transparency color

    public Color Foreground { get; set; }                       = new Color(255, 255, 255, 255);    // Window foreground color
    public Color Background { get; set; }                       = new Color(50, 55, 65, 255);       // Window background color

    public Color ButtonForeground { get; set; }                 = new Color(255, 255, 255, 255);    // Button foreground color
    public Color ButtonBackground { get; set; }                 = new Color(90, 95, 105, 255);      // Button background color

    public Color ButtonHoverBackground { get; set; }            = new Color(110, 115, 125, 255);    // Button hover background color
    public Color ButtonActiveBackground { get; set; }           = new Color(80, 85, 95, 255);       // Button active background color

    public Color HeaderForeground { get; set; }                 = new Color(255, 255, 255, 255);    // Header foreground color
    public Color HeaderBackground { get; set; }                 = new Color(90, 95, 105, 255);      // Header background color

    public Color TooltipForeground { get; set; }                = new Color(255, 255, 255, 255);    // Tooltip foreground color
    public Color TooltipBackground { get; set; }                = new Color(90, 95, 105, 255);      // Tooltip background color

    public Font Font { get; private set; }                                                          // Font used for all text
    public float FontSize { get { return Font.baseSize; } }                                         // Base size of the font
    public int FontSpacing { get; private set; } = 3;                                               // Space between glyphs

    public Theme(string font_path) {
        Font = LoadFont(font_path);
    }
}