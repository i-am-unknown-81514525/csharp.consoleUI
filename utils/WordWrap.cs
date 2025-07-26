using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ui.utils
{
    public class TextFlag
    {
        public const int MULTILINE = 1;
        public const int TEXTWRAP = 2;

        private int v { get; }

        public TextFlag(int v)
        {
            this.v = v;
        }

        public static implicit operator TextFlag(int v) => new TextFlag(v);

        public static implicit operator int(TextFlag flag) => flag.v;
    }

    public static class WordWrapExtension {
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
    }
}