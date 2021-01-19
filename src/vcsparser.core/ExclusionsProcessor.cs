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

        public ExclusionsProcessor(string expressions)
        {
            if (String.IsNullOrEmpty(expressions))
                this.exclusionExpressions = new List<Regex>();
            else
                this.exclusionExpressions = expressions.Split(',').Select(exp => FileSystemExpressionToRegexConverter.ToRegex(true, exp)).ToList<Regex>();
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
