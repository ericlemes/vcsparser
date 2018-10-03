using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public interface IJsonExporter
    {
        void Export(SonarMeasuresJson measures, string outputFile);
    }
}
