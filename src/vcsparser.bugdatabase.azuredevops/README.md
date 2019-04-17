# Azure DevOps

Bug Database Plugin to connect to Azure DevOps work items

## Usage

Parse the dll to either the `p4extract` or `gitextract` commands and supply the following arguments

```
--organisation    Required. Organisation of where project exists

--project         Required. Project to query

--team            Required. Team Project

--query           Query to select work Items

--from            Required. Start Date in format 'yyyy-mm-dd'

--to              Required. End Date in format 'yyyy-mm-dd'

--token           Required. Azure DevOps Personal Access Token

--help            Display this help screen.

--version         Display version information.
```