using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public interface IMeasureConverter
    {
        void ProcessFileMeasure(DailyCodeChurn dailyCodeChurn, SonarMeasuresJson sonarMeasuresJson);
        void ProcessProjectMeasure(SonarMeasuresJson sonarMeasuresJson);

        DateTime StartDate { get; }
        DateTime EndDate { get; }
    }
}
