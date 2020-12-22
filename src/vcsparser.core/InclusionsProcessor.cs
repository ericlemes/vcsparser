using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class InclusionsProcessor
    {
        private List<Regex> inclusionExpressions;

        public InclusionsProcessor(string expressions)
        {
            if (String.IsNullOrEmpty(expressions))
                this.inclusionExpressions = new List<Regex>();
            else
                this.inclusionExpressions = expressions.Split(',').Select(exp => FileSystemExpressionToRegexConverter.ToRegex(false, exp)).ToList<Regex>();
        }

        public bool IsIncluded(string fileName)
        {
            foreach (var regex in inclusionExpressions)
            {
                if (regex.IsMatch(fileName))
                    return true;
            }
            return false;
        }
    }
}
