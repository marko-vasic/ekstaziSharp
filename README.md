# Ekstazi# - Regression Test Selection for C#

## Running

Try Ekstazi#
example.sh script provides an usage example of Ekstazi#. The script
does following: 1) builds Ekstazi# project if it is not built. 2)
clones FluentValidation project from github which will be used as an
example project under test. 3) Runs tests on FluentValidation using
Ekstazi#. Note that the first time all tests will be run in
FluentValidation project. Afterwards, 0 tests should be run unless you
modify FluentValidation project.

More information about different settings for running Ekstazi# are
work in progress, please stay tuned, or feel free to contact the
authors.

## Citing

If you would like to reference Ekstazi# in an academic publication, please cite our FSE '17 publication:
```

@inproceedings{ekstaziSharp,
	title = {File-Level vs. Module-Level Regression Test Selection for .NET},
	author = {Vasic, Marko and Parvez, Zuhair and Milicevic, Aleksandar and Gligoric, Milos},
        booktitle = {Proceedings of the 2017 25th ACM SIGSOFT International Symposium on Foundations of Software Engineering},
        series = {FSE 2017},
        year = {2017}
}
```

## Acknowledgements

We thank the fellow students of EE 382C (Verification and Validation
of Software) at The University of Texas at Austin for constructive
discussions on the material presented in this paper. We also thank
Ahmet Celik, Nima Dini, and Sarfraz Khurshid for their feedback on
this work.  This research was partially supported by the US National
Science Foundation under Grants Nos.~CCF-1566363 and CCF-1652517.