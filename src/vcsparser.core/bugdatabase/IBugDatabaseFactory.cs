using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public interface IBugDatabaseFactory
    {
        IHttpClientWrapper GetHttpClientWrapper();

        Assembly LoadFile(string file);
        IBugDatabaseProvider CreateInstance(Type type);
    }
}
