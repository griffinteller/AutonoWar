using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UI
{
    public class ColoredString
    {
        public string RenderedString { get; }
        public string RawString { get; }
        public int RenderedLength => RenderedString.Length;

        private ColoredString(string renderedString, string rawString)
        {
            RenderedString = renderedString;
            RawString = rawString;
        }

        public ColoredString(string rawString)
        {
            RawString = rawString;
            RenderedString = Regex.Replace(
                rawString, @"(<color=#\d\d\d\d\d\d>)|(<\\color>)", "");
        }

        public ColoredString(string renderedString, int defaultColor = 0, (int, int, int)[] colorAndRanges = null)
        {
            colorAndRanges = colorAndRanges ?? new (int, int, int)[0];

            StringBuilder rawStringBuilder = new StringBuilder(renderedString);

            foreach ((int color, int start, int end) in colorAndRanges) // add tags for given ranges
            {
                rawStringBuilder.Insert(start, "<color=#" + color.ToString("X6") + ">");
                rawStringBuilder.Insert(end, "</color>");
            }

            rawStringBuilder.Insert(0, "<color=#" + defaultColor.ToString("X6") + ">"); // add default color tags
            rawStringBuilder.Append("</color>");

            RenderedString = renderedString;
            RawString = rawStringBuilder.ToString();
        }

        public static ColoredString operator +(ColoredString str1, ColoredString str2)
        {
            return Concat(str1, str2);
        }

        private static ColoredString Concat(ColoredString str1, ColoredString str2)
        {
            return new ColoredString(
                str1.RenderedString + str2.RenderedString,
                str1.RawString + str2.RawString);
        }

        public static ColoredString[] ColorStringArray(string[] arr, int color = 0)
        {
            ColoredString[] result = new ColoredString[arr.Length];
            
            for (int i = 0; i < arr.Length; i++)
                result[i] = new ColoredString(arr[i], color);
            
            return result;
        }

        public override string ToString()
        {
            return RawString;
        }

        public static int HexFromColor(Color color)
        {
            return ((int) (color.r * 255) << 16) + ((int) (color.g * 255) << 8) + (int) (color.b * 255);
        }
    }
}