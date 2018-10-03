using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public interface IMeasureConverter
    {
        void Process(DailyCodeChurn dailyCodeChurn, SonarMeasuresJson sonarMeasuresJson);
    }
}
