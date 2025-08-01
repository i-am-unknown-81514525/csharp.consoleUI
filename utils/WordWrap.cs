using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ui.utils
{

    public enum VerticalAlignment : int
    {
        TOP = 0,
        MIDDLE = 1,
        BOTTOM = 2,
    }
    public enum HorizontalAlignment : int
    {
        LEFT = 0,
        MIDDLE = 1,
        RIGHT = 2,
    }

    public static class WordWrapExtension
    {
        public static string Wrap(this string src, int amount, string weakSplitOptions = " .,/?!<>:;", string stripWith = " ")
        {
            if (amount < 1) throw new InvalidOperationException("The amount cannot be <= 0 as there are no method to make a valid split");
            StringBuilder output = new StringBuilder();
            StringBuilder currLine = new StringBuilder();
            StringBuilder tmp = new StringBuilder();
            int lineCount = 0;
            int lineCountWithTmp = 0;
            bool appliedNewLine = true;
            bool justEol = true;
            char[] stripArr = stripWith.ToArray();
            foreach (char v in src)
            {
                if (lineCountWithTmp >= amount)
                {
                    if (lineCount != 0)
                    {
                        if (!appliedNewLine)
                        {
                            output.Append("\n");
                        }
                        appliedNewLine = false;
                        output.Append(currLine.ToString().Trim(stripArr));
                        lineCountWithTmp -= lineCount;
                        lineCount = 0;
                        currLine = new StringBuilder();
                        justEol = true;
                    }
                    else
                    {
                        if (!appliedNewLine)
                        {
                            output.Append("\n");
                        }
                        appliedNewLine = false;
                        output.Append(tmp.ToString().Substring(0, amount).Trim(stripArr));
                        tmp = new StringBuilder(tmp.ToString().Substring(amount));
                        lineCountWithTmp -= amount;
                        justEol = true;
                    }
                }
                if (v == '\n')
                {
                    if (!justEol && !appliedNewLine)
                    {
                        output.Append("\n");
                    }
                    output.Append(currLine);
                    output.Append(tmp);
                    output.Append("\n");
                    currLine = new StringBuilder();
                    tmp = new StringBuilder();
                    appliedNewLine = true;
                    lineCount = 0;
                    lineCountWithTmp = 0;
                    justEol = true;
                    continue;
                }
                if (!((lineCountWithTmp == 0 || lineCountWithTmp == amount) && stripWith.Contains(v)))
                {
                    lineCountWithTmp++;
                    tmp.Append(v);
                }
                justEol = false;
                if (weakSplitOptions.Contains(v))
                {
                    currLine.Append(tmp);
                    tmp = new StringBuilder();
                    lineCount = lineCountWithTmp;
                }
            }
            if (lineCountWithTmp - lineCount > 0)
            {
                currLine.Append(tmp);
                lineCount = lineCountWithTmp;
            }
            if (lineCount > 0)
                if (!appliedNewLine)
                {
                    output.Append("\n");
                }
            output.Append(currLine.ToString().Trim(stripArr));
            return output.ToString();
        }

        public static string Align(this string src, (VerticalAlignment align, int space) vAlign, (HorizontalAlignment align, int space) hAlign)
        {
            // Note that this can truncate if insufficient space
            StringBuilder outputStringBuilder = new StringBuilder((hAlign.space + 1) * vAlign.space);
            string[] splitted = src.Split('\n');
            int vLength = splitted.Length;
            for (int y = 0; y < vAlign.space; y++)
            {
                int vIdx = y;
                int vOffset = (vAlign.space - vLength);
                if (vOffset < 0) vOffset = 0;
                switch (vAlign.align)
                {
                    case VerticalAlignment.TOP:
                        vIdx = y;
                        break;
                    case VerticalAlignment.MIDDLE:
                        vIdx = y - (vOffset / 2);
                        break;
                    case VerticalAlignment.BOTTOM:
                        vIdx = y - (vOffset);
                        break;
                }
                string lineContent = "";
                if (vIdx < vLength && vIdx >= 0)
                {
                    lineContent = splitted[vIdx];
                }
                int hLength = lineContent.Length;
                if (lineContent == "")
                {
                    outputStringBuilder.Append(new string(' ', hAlign.space) + "\n");
                    continue;
                }
                for (int x = 0; x < hAlign.space; x++)
                {
                    int hIdx = x;
                    int hOffset = (hAlign.space - hLength);
                    if (hOffset < 0) hOffset = 0;
                    switch (hAlign.align)
                    {
                        case HorizontalAlignment.LEFT:
                            hIdx = x;
                            break;
                        case HorizontalAlignment.MIDDLE:
                            hIdx = x - (hOffset / 2);
                            break;
                        case HorizontalAlignment.RIGHT:
                            hIdx = x - hOffset;
                            break;
                    }
                    char v = ' ';
                    if (hIdx < hLength && hIdx >= 0)
                    {
                        v = lineContent[hIdx];
                    }
                    if (y + 1 == vAlign.space && vLength > vAlign.space && hIdx + 1 == hLength) // Final line, with more line exist that is truncated, and the final character
                    {
                        v = SpecialChar.SINGLE_CHAR_ELLIPSIS;
                    }
                    if (x + 1 == hAlign.space && hLength > hAlign.space) // Final character with more character exist that is truncated
                    {
                        v = SpecialChar.SINGLE_CHAR_ELLIPSIS;
                    }
                    outputStringBuilder.Append(v);
                }
                outputStringBuilder.Append("\n");
            }
            return outputStringBuilder.ToString();
        }

        public static string Align(this string src, (HorizontalAlignment align, int space) hAlign, (VerticalAlignment align, int space) vAlign)
        {
            return src.Align(vAlign, hAlign);
        }
    }
}
