﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.bugdatabase;

namespace vcsparser.bugdatabase.azuredevops
{
    public class AzureDevOpsFactory : IAzureDevOpsFactory
    {
        public IAzureDevOps GetAzureDevOps(ILogger logger, IAzureDevOpsRequest request, IApiConverter apiConverter, ITimeKeeper timeKeeper)
        {
            return new AzureDevOps(logger, request, apiConverter, timeKeeper);
        }
    }
}
