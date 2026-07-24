using System.Collections.Generic;
using UnityEngine;

namespace Countdown.Placeholder
{
    // Placeholder hex swatches for the reagent colors (color is diagnosis-only, never
    // blood-testable, so this is only ever used for the Book's color entry and the
    // Synthesis color shelf - never for the blood glyph).
    public static class ColorPalette
    {
        private static readonly Dictionary<string, Color> Colors = new()
        {
            ["crimson"] = HexColor("#DC143C"),
            ["amber"] = HexColor("#FFBF00"),
            ["azure"] = HexColor("#007FFF"),
            ["violet"] = HexColor("#8F00FF"),
            ["jade"] = HexColor("#00A86B"),
            ["ash"] = HexColor("#888888"), // decoy, never correct
        };

        public static Color Get(string name) =>
            Colors.TryGetValue(name, out var c) ? c : Color.magenta; // magenta = missing-mapping flag

        private static Color HexColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
