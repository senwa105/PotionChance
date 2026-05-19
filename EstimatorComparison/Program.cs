using System.CommandLine;
using EstimatorComparison;

Argument<FileInfo> runFileArgument = new("--run-file")
{
    Description = "The path to the .run file to simulate."
};

Option<int> numPassesOption = new("--num-passes", "-n")
{
    Description = "Number of simulation passes to perform.",
    DefaultValueFactory = parseResult => 100_000
};

RootCommand rootCommand = new RootCommand("Compare HMM and STS1 Estimators");
rootCommand.Arguments.Add(runFileArgument);
rootCommand.Options.Add(numPassesOption);

rootCommand.SetAction(parseResult =>
{
    FileInfo runFile = parseResult.GetValue(runFileArgument)!;
    int numPasses = parseResult.GetValue(numPassesOption);

    if (!runFile.Exists)
    {
        Console.WriteLine($"Error: Run file not found at '{runFile.FullName}'");
        return;
    }

    Console.WriteLine($"Simulating {numPasses:N0} passes for {runFile.Name}...");

    SimulationResults simResults = RunSimulator.Simulate(runFile.FullName, numPasses);
    
    double hmmMeanBrier = simResults.HmmScores.Average();
    double sts1MeanBrier = simResults.Sts1Scores.Average();
    double skillScore = (sts1MeanBrier - hmmMeanBrier) / sts1MeanBrier;

    TTestResults tTestResults = RunSimulator.PairedTTest(
        simResults.HmmScores.ToArray(), 
        simResults.Sts1Scores.ToArray()
    );

    Console.WriteLine("\n=== SIMULATION RESULTS ===");
    Console.WriteLine($"Mean HMM Brier Score:  {hmmMeanBrier:F5}");
    Console.WriteLine($"Mean STS1 Brier Score: {sts1MeanBrier:F5}");
    Console.WriteLine($"Brier Skill Score:     {skillScore:P2}");

    Console.WriteLine("\n=== STATISTICAL ANALYSIS (Paired t-test) ===");
    Console.WriteLine($"Mean Difference:       {tTestResults.MeanDiff:F5}");
    Console.WriteLine($"Standard Deviation:    {tTestResults.StdDev:F5}");
    Console.WriteLine($"t-statistic:           {tTestResults.TStat:F2}");
});

return rootCommand.Parse(args).Invoke();
