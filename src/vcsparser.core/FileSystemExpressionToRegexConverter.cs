using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class FileSystemExpressionToRegexConverter
    {
        private static readonly string SPECIAL_CHARS = "()[]^$.{}+|";

        public static Regex ToRegex(bool caseSensitive, string exp)
        {
            StringBuilder regex = new StringBuilder(exp.Length);

            if (!caseSensitive)
            {
                regex.Append("(?i)");
            }

            int i = 0;
            while (i < exp.Length)
            {
                if (!ProcessedSpecialChar(exp, regex, ref i))
                    if (!ProcessedAster(exp, regex, ref i))
                        if (!ProcessedQuestionMark(exp, regex, ref i))
                        {
                            //Process non-special char
                            regex.Append(exp[i]);
                            i++;
                        }
            }

            regex.Append("$");

            return new Regex(regex.ToString());
        }

        private static bool ProcessedSpecialChar(string exp, StringBuilder regex, ref int i)
        {
            if (SPECIAL_CHARS.IndexOf(exp[i]) <= 0)
                return false;

            //Escape
            regex.Append(@"\");
            regex.Append(exp[i]);
            i++;

            return true;
        }

        private static bool ProcessedAster(string exp, StringBuilder regex, ref int i)
        {
            if (exp[i] != '*')
                return false;

            if ((i + 1 < exp.Length && exp[i + 1] == '*') && (i + 2 < exp.Length && exp[i + 2] == '/'))
            {
                //Double aster followed by a slash
                regex.Append("(.*|.*/)");
                i += 3;
            }
            else
            {
                //Single aster
                regex.Append(".*");
                i++;
            }

            return true;
        }

        private static bool ProcessedQuestionMark(string exp, StringBuilder regex, ref int i)
        {
            if (exp[i] != '?')
                return false;

            regex.Append("(.{1}|^/)");
            i++;
            return true;
        }
    }
}
