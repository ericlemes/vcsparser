using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.p4
{
    public class ChangesParser : IChangesParser
    {
        public List<int> Parse(List<string> lines)
        {
            var result = new List<int>();
            foreach (var line in lines)
            {
                result.Add(ParseLine(line));
            }

            return result;
        }

        private int ParseLine(string line)
        {
            var splittedString = line.Split(' ');
            if (splittedString.Length < 2 || splittedString[0] != "Change")
                throw new InvalidFormatException("Invalid file format. Expecting lines with Change <number>");

            try
            {
                return Convert.ToInt32(splittedString[1]);
            }
            catch (FormatException){
                throw new InvalidFormatException("Invalid file format. Expecting lines with Change <number>");
            }
        }
    }
}
