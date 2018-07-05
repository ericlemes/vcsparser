using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
{
    public interface IDescribeParser
    {
        Changeset Parse(Stream ms);
    }
}
