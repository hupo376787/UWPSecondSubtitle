using System;
using System.Text.RegularExpressions;

namespace UWPSecondSubtile
{
    public class ColorHelper
    {
        private static Regex _hexColorMatchRegex = new Regex("^#?(?<a>[a-z0-9][a-z0-9])?(?<r>[a-z0-9][a-z0-9])(?<g>[a-z0-9][a-z0-9])(?<b>[a-z0-9][a-z0-9])$", RegexOptions.IgnoreCase);

        public static Windows.UI.Color GetColorFromHex(string hexColorString)
        {
            if (hexColorString == null)
                throw new NullReferenceException("Hex string can't be null.");

            var match = _hexColorMatchRegex.Match(hexColorString);
            if (!match.Success)
                throw new InvalidCastException(string.Format("Can't convert string \"{0}\" to argb or rgb color. Needs to be 6 (rgb) or 8 (argb) hex characters long. It can optionally start with a #.", hexColorString));

            byte a = 255, r = 0, b = 0, g = 0;
            if (match.Groups["a"].Success)
                a = System.Convert.ToByte(match.Groups["a"].Value, 16);
            r = System.Convert.ToByte(match.Groups["r"].Value, 16);
            b = System.Convert.ToByte(match.Groups["b"].Value, 16);
            g = System.Convert.ToByte(match.Groups["g"].Value, 16);
            return Windows.UI.Color.FromArgb(a, r, g, b);
        }

    }
}
