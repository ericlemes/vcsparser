using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class ExclusionsProcessor : IExclusionsProcessor
    {
        private List<Regex> exclusionExpressions;
        private readonly string SPECIAL_CHARS = "()[]^$.{}+|";

        public ExclusionsProcessor(string expressions)
        {
            if (String.IsNullOrEmpty(expressions))
                this.exclusionExpressions = new List<Regex>();
            else
                this.exclusionExpressions = expressions.Split(',').Select(exp => ToRegex(exp)).ToList<Regex>();
        }

        private Regex ToRegex(string exp)
        {
            StringBuilder regex = new StringBuilder(exp.Length);

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

            return new Regex(regex.ToString());
        }

        private bool ProcessedSpecialChar(string exp, StringBuilder regex, ref int i)
        {
            if (SPECIAL_CHARS.IndexOf(exp[i]) <= 0)
                return false;
            
            //Escape
            regex.Append(@"\");
            regex.Append(exp[i]);
            i++;

            return true;
        }

        private bool ProcessedAster(string exp, StringBuilder regex, ref int i)
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

        private bool ProcessedQuestionMark(string exp, StringBuilder regex, ref int i)
        {
            if (exp[1] != '?')
                return false;
            
            regex.Append("(.{1}|^/)");
            i++;
            return true;
        }

        public bool IsExcluded(string fileName)
        {
            foreach(var regex in exclusionExpressions)
            {
                if (regex.IsMatch(fileName))
                    return true;
            }
            return false;
        }
    }
}
