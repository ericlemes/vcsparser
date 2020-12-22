using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class FilePrefixRemover
    {
        public string ProcessFileName(string fileName, string filePrefixToRemove)
        {
            if (filePrefixToRemove == null)
                return fileName;

            if (fileName.StartsWith(filePrefixToRemove, StringComparison.OrdinalIgnoreCase))
                return fileName.Substring(filePrefixToRemove.Length);

            return fileName;
        }
    }
}
