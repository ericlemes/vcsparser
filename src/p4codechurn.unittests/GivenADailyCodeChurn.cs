using p4codechurn.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace p4codechurn.unittests
{
    public class GivenADailyCodeChurn
    {
        [Fact]
        public void WhenGettingExtensionShouldReturnExpectedValue()
        {
            var churn = new DailyCodeChurn();
            churn.FileName = "//somepath/bin/Latest/BuildSystem/Wrappers/DatabaseExport.ps1";
            Assert.Equal(".ps1", churn.Extension);
        }

        [Fact]
        public void WhenGettingExtensionAndFileNameNullShouldReturnExpectedValue()
        {
            var churn = new DailyCodeChurn();
            churn.FileName = null;
            Assert.Null(churn.Extension);
        }

        [Fact]
        public void WhenGettingExtensionAndFileNameEmptyShouldReturnExpectedValue()
        {
            var churn = new DailyCodeChurn();
            churn.FileName = "";
            Assert.Equal("", churn.Extension);
        }

        [Fact]
        public void WhenGettingExtensionForBigFilePathShouldReturnExpectedValue()
        {
            var churn = new DailyCodeChurn();
            churn.FileName = "//sunrise/bin/Latest/Tools/AssemblyCache/GameTestLibrary.Opus.TestDependencies/AnyCPU/v1/GameTestLibrary.Opus.Exceptions.dll";
            Assert.Equal(".dll", churn.Extension);
        }

        [Fact]
        public void WhenComparingAndDatesAreDifferentShouldConsiderDate()
        {
            var churn1 = new DailyCodeChurn { Timestamp = new DateTime(2018, 9, 5) };
            var churn2 = new DailyCodeChurn { Timestamp = new DateTime(2018, 9, 4) };
            Assert.Equal(-1, churn2.CompareTo(churn1));
        }

        [Fact]
        public void WhenComparingAndDatesAreEqualShouldConsiderFileName()
        {
            var churn1 = new DailyCodeChurn { FileName = "b" };
            var churn2 = new DailyCodeChurn { FileName = "a" };
            Assert.Equal(-1, churn2.CompareTo(churn1));
        }
    }
}
