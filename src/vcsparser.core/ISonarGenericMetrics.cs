using System;

namespace vcsparser.core
{
    public interface ISonarGenericMetrics
    {
        string FilePrefixToRemove { get; set; }

        DateTime? EndDate { get; set; }

        string Generate1Year { get; set; }

        string Generate6Months { get; set; }

        string Generate3Months { get; set; }

        string Generate30Days { get; set; }

        string Generate7Days { get; set; }

        string Generate1Day { get; set; }
    }
}
