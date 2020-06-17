using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UWPSecondSubtile
{
    public class StringHelper
    {
        public static string RemoveHtmlTags(string input, bool fixInvalidTags = false)
        {
            string returnVal = input ?? "";
            if (fixInvalidTags)
            {
                returnVal = FixInvalidItalicTags(returnVal);
                returnVal = FixInvalidFontTags(returnVal);
                returnVal = FixInvalidUnderlineTags(returnVal);
                returnVal = FixInvalidBoldTags(returnVal);
            }
            returnVal = Regex.Replace(returnVal, "</?(font).*?>", "");
            returnVal = Regex.Replace(returnVal, "</?(i).*?>", "");
            returnVal = Regex.Replace(returnVal, "</?(u).*?>", "");
            returnVal = Regex.Replace(returnVal, "</?(b).*?>", "");

            return returnVal;
        }
        public static string FixInvalidItalicTags(string text)
        {
            const string beginTag = "<i>";
            const string endTag = "</i>";
            text = text.Replace("<I>", beginTag);
            text = text.Replace("< i >", beginTag);
            text = text.Replace("< i>", beginTag);
            text = text.Replace("<i >", beginTag);
            text = text.Replace("< I>", beginTag);
            text = text.Replace("<I >", beginTag);

            text = text.Replace("</I>", endTag);
            text = text.Replace("< / i >", endTag);
            text = text.Replace("< /i>", endTag);
            text = text.Replace("</ i>", endTag);
            text = text.Replace("< /i>", endTag);
            text = text.Replace("< /i >", endTag);
            text = text.Replace("</i >", endTag);
            text = text.Replace("</ i >", endTag);
            text = text.Replace("< / i>", endTag);
            text = text.Replace("< /I>", endTag);
            text = text.Replace("</ I>", endTag);
            text = text.Replace("< /I>", endTag);
            text = text.Replace("< / I >", endTag);
            return text;
        }
        public static string FixInvalidFontTags(string text)
        {
            const string beginTag = "<font";
            const string endTag = "</font>";
            text = text.Replace("<Font", beginTag);
            text = text.Replace("<FONT", beginTag);

            text = text.Replace("</Font>", endTag);
            text = text.Replace("</FONT>", endTag);
            return text;
        }
        public static string FixInvalidUnderlineTags(string text)
        {
            const string beginTag = "<u>";
            const string endTag = "</u>";
            text = text.Replace("<U>", beginTag);
            text = text.Replace("< u >", beginTag);
            text = text.Replace("< u>", beginTag);
            text = text.Replace("<u >", beginTag);
            text = text.Replace("< U>", beginTag);
            text = text.Replace("<U >", beginTag);

            text = text.Replace("</U>", endTag);
            text = text.Replace("< / u >", endTag);
            text = text.Replace("< /u>", endTag);
            text = text.Replace("</ u>", endTag);
            text = text.Replace("< /u>", endTag);
            text = text.Replace("< /u >", endTag);
            text = text.Replace("</u >", endTag);
            text = text.Replace("</ u >", endTag);
            text = text.Replace("< / u>", endTag);
            text = text.Replace("< /U>", endTag);
            text = text.Replace("</ U>", endTag);
            text = text.Replace("< /U>", endTag);
            text = text.Replace("< / U >", endTag);
            return text;
        }
        public static string FixInvalidBoldTags(string text)
        {
            const string beginTag = "<b>";
            const string endTag = "</b>";
            text = text.Replace("<B>", beginTag);
            text = text.Replace("< b >", beginTag);
            text = text.Replace("< b>", beginTag);
            text = text.Replace("<b >", beginTag);
            text = text.Replace("< B>", beginTag);
            text = text.Replace("<B >", beginTag);

            text = text.Replace("</B>", endTag);
            text = text.Replace("< / b >", endTag);
            text = text.Replace("< /b>", endTag);
            text = text.Replace("</ b>", endTag);
            text = text.Replace("< /b>", endTag);
            text = text.Replace("< /b >", endTag);
            text = text.Replace("</b >", endTag);
            text = text.Replace("</ b >", endTag);
            text = text.Replace("< / b>", endTag);
            text = text.Replace("< /B>", endTag);
            text = text.Replace("</ B>", endTag);
            text = text.Replace("< /B>", endTag);
            text = text.Replace("< / B >", endTag);
            return text;
        }
    }
}
