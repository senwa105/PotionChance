This is a quick and dirty CLI tool to compare the predictive performance of the `HMM` and `STS1` estimators.

**TLDR**: The `HMM` estimator improves on the `STS1` estimator by only about 5-8%. 

### What it does
A `.run` file is parsed for an example of realistic pathing.
We then run potion drop simulations (by default 100,000 trials) on this path while keeping track of the estimated chance computed by the estimators.
The estimators are compared at the end using the [Brier score](https://en.wikipedia.org/wiki/Brier_score) (lower is better).
(We also run a paired t-test to check the reliability of the results, but it turns out 100,000 trials is enough for essentially zero p score.)

A very unscientific sample of my last few winning Ironclad A10 runs are provided as sample `.run` files.

### How to run
```
dotnet run -- -help
Description:
  Compare HMM and STS1 Estimators

Usage:
  EstimatorComparison <--run-file> [options]

Arguments:
  <--run-file>  The path to the .run file to simulate.

Options:
  -n, --num-passes <num-passes>  Number of simulation passes to perform. [default: 100000]
  -?, -h, --help                 Show help and usage information
  --version                      Show version information
```

### Sample output
```
dotnet run -- runs/1778126605.run
Simulating 100,000 passes for 1778126605.run...

=== SIMULATION RESULTS ===
Mean HMM Brier Score:  0.23112
Mean STS1 Brier Score: 0.25049
Brier Skill Score:     7.73%

=== STATISTICAL ANALYSIS (Paired t-test) ===
Mean Difference:       -0.01937
Standard Deviation:    0.04066
t-statistic:           -150.67
```

On the included sample of runs, the Brier skill score (percent reduction in Brier score) of `HMM` relative to `STS1` ranges from 5.41% to 8.00% with mean 6.83 and std. dev. 1.08.
Given how small this improvement is, players who do not want to use a mod would lose very little by manually calculating the `STS1` estimator.
