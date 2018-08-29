# p4codechurn

Command line tool to extract code churn metrics from p4 repositories. Parses the output of p4 command line and outputs a csv file with the number of changes per file.


# Status

| | |
| --- | --- |
| **Build** | ![Build status](https://img.shields.io/appveyor/ci/ericlemes/p4codechurn.svg)|
| **Coverage** | [![codecov](https://codecov.io/gh/ericlemes/p4codechurn/branch/master/graph/badge.svg)](https://codecov.io/gh/ericlemes/p4codechurn) |


# Usage

```
  --changes     Required. p4 changes command line to get changesets. Usually "p4 changes -s submitted
                //path/to/your/depot/...@YYYY/MM/DD,YYYY/MM/DD" or something similar
  
  --describe    Required. p4 describe command line to describe every changeset. Usually "p4 describe -ds {0}" should
                work. {0} will be substituted by the change number during execution
  
  --output      path to dump csv output
  
  --help        Display this help screen.
  
  --version     Display version information.
```
