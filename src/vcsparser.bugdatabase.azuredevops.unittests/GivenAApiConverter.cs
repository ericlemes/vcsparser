using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;
using Xunit;

namespace vcsparser.bugdatabase.azuredevops.unittests
{
    public class GivenAApiConverter
    {
        private IApiConverter apiConverter;

        public GivenAApiConverter()
        {
            apiConverter = new ApiConverter();
        }

        [Fact]
        public void WhenConvertingThenItemWithValues()
        {
            var fullItem = new JObject
            {
                { "id", "Some Work Item id" },
                { "fields", new JObject
                    {
                        { "Microsoft.VSTS.Build.IntegrationBuild", "Some Integration Build" },
                        { "Microsoft.VSTS.Common.ClosedDate", "2019-04-15T13:47:30.00Z"}
                    }
                }
            };

            WorkItem workItem = apiConverter.ConvertToWorkItem(fullItem.ToObject<object>());

            Assert.Equal(fullItem["id"].ToString(), workItem.WorkItemId);
            Assert.Equal(fullItem["fields"]["Microsoft.VSTS.Build.IntegrationBuild"].ToString(), workItem.ChangesetId);
            Assert.Equal(DateTime.Parse(fullItem["fields"]["Microsoft.VSTS.Common.ClosedDate"].ToString()), workItem.ClosedDate);
        }

        [Fact]
        public void WhenConvertingMissingStringEntryThenPropertyNull()
        {
            var fullItem = new JObject
            {
                { "id", "Some Work Item id" },
                { "fields", new JObject
                    {
                        { "Microsoft.VSTS.Common.ClosedDate", "2019-04-15T13:47:30.00Z"}
                    }
                }
            };

            WorkItem workItem = apiConverter.ConvertToWorkItem(fullItem.ToObject<object>());

            Assert.Null(workItem.ChangesetId);
        }

        [Fact]
        public void WhenConvertingBadDateThenPropertyNull()
        {
            var fullItem = new JObject
            {
                { "id", "Some Work Item id" },
                { "fields", new JObject
                    {
                        { "Microsoft.VSTS.Build.IntegrationBuild", "Some Integration Build" },
                        { "Microsoft.VSTS.Common.ClosedDate", "This is not a Date"}
                    }
                }
            };

            Action action = () => apiConverter.ConvertToWorkItem(fullItem.ToObject<object>());

            var exception = Assert.Throws<FormatException>(action);
            Assert.StartsWith("The string was not recognized as a valid DateTime.", exception.Message);
        }

        [Fact]
        public void WhenConvertingMissingDateThenPropertyNull()
        {
            var fullItem = new JObject
            {
                { "id", "Some Work Item id" },
                { "fields", new JObject
                    {
                        { "Microsoft.VSTS.Build.IntegrationBuild", "Some Integration Build" }
                    }
                }
            };

            Action action = () => apiConverter.ConvertToWorkItem(fullItem.ToObject<object>());

            var exception = Assert.Throws<ArgumentNullException>(action);
        }
    }
}
