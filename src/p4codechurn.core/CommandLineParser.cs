using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
{
    public class CommandLineParser : ICommandLineParser
    {
        public Tuple<string, string> ParseCommandLine(string commandLine)
        {
            int index = 0;
            bool openQuote = false;

            string item1 = "", item2 = "";

            foreach(var c in commandLine.ToCharArray())
            {
                if (c == ' ' && !openQuote)
                {
                    item1 = commandLine.Substring(0, index);
                    item2 = commandLine.Substring(index + 1);
                    return new Tuple<string, string>(RemoveQuotes(commandLine.Substring(0, index)), commandLine.Substring(index + 1));
                }
                else if (c == '\"')
                    openQuote = !openQuote;
                index++;
            }
            return new Tuple<string, string>(RemoveQuotes(commandLine), "");
        }

        private string RemoveQuotes(string s)
        {
            if (s == "\"" || s == "\"\"")
                return "";

            if (s.Length < 2)
                return s;

            if (s[0] == '\"' && s[s.Length - 1] == '\"')
                return s.Substring(1, s.Length - 2);

            return s;
        }
    }
}
