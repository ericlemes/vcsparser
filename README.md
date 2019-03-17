# vcsparser

Command line tool to extract code churn metrics from p4 (Perforce) and Git repositories. Parses the output of p4 command line or git log command line and outputs a json file with the number of changes per file.

Also reads back the json files and outputs a json file that can be integrated with SonarQube.


# Status

| | |
| --- | --- |
| **Build** | ![Build status](https://img.shields.io/appveyor/ci/ericlemes/p4codechurn.svg)|
| **Coverage** | [![codecov](https://codecov.io/gh/ericlemes/vcsparser/branch/master/graph/badge.svg)](https://codecov.io/gh/ericlemes/p4codechurn) |


# Why?

The idea of this tool is to extract code churn information (number of changes and number of lines changed) for Perforce and Git repositories and either using this information (json format) or publishing to SonarQube repositories.

It seems to be a pretty straightforward task, but since P4 is really slow to extract this information, some workarounds are necessary. Also for Git there are some complexities like file renames which makes the task slightly more difficult (renames are handled).
 
The main use case is:

- Extract from p4 or git repositories and keeping the daily json files on disk
- Reading files from disk (appending this information every day) and exporting to a .json file (Sonar Generic Metrics format. https://github.com/ericlemes/sonar-generic-metrics).
- Publishing to SonarQube using Sonar Generic Metrics.


# Some important information

When exporting to SonarQube, it requires that the files referenced inside your .json file is found during the SonarQube analysis. That's what the --fileprefixtoremove is for. The same option --fileprefixtoremove is useless for git, since it considers the root by default.


# Usage

There are 3 commands for p4codechurn:

```
  p4extract              Extracts code coverage information from p4 and outputs to json

  gitextract             Extracts code coverage information from git log file and outputs to json

  sonargenericmetrics    Process json files and outputs to Sonar Generic Metrics JSON format
```

The p4extract command is used to read data from perforce and save to a .json file. 

```
  --changes     Required. p4 changes command line to get changesets. Usually "p4 changes -s submitted
                //path/to/your/depot/...@YYYY/MM/DD,YYYY/MM/DD" or something similar
  
  --describe    Required. p4 describe command line to describe every changeset. Usually "p4 describe -ds {0}" should
                work. {0} will be substituted by the change number during execution
  
  --output      path to dump json output
  
  --help        Display this help screen.
  
  --version     Display version information.
```

The gitextract command is used to read data from Git repositores and save to a .json file 

```
  --gitlogcommand    Required. Command line that will be invoked to get git log. Syntax should be similar to: git log
                     --pretty=fuller --date=iso --after=YYYY-MM-DD --numstat

  --output           Required. File path for single file or file prefix for multiple files.

  --output-type      Required. SingleFile or MultipleFile. MultipleFile dumps one file per date.

  --help             Display this help screen.

  --version          Display version information.
```

***It is very important to use the order provided by git by default (newer commits in the beginning). This is necessary to handle renames properly.***

The sonargenericmetrics reads json files and exports to a .json file in the expected structure of Sonar Generic Metrics (https://github.com/ericlemes/sonar-generic-metrics). 

```
  --fileprefixtoremove    Required. Prefix to remove from file. Usually repository root

  --inputdir              Required. Directory with input json files

  --outputfile            Required. File to generate json output

  --enddate               Date to limit the analysis to.

  --generate1year         (Default: true) Generates 1 year churn data.

  --generate6months       (Default: true) Generates 6 months churn data.

  --generate3months       (Default: true) Generates 3 months churn data.

  --generate30days        (Default: true) Generates 30 days churn data.

  --generate7days         (Default: true) Generates 7 days churn data.

  --generate1day          (Default: true) Generates 1 day churn data.

  --help                  Display this help screen.

  --version               Display version information.
  
 ```
