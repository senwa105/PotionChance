using System.Collections.Immutable;
using PotionChanceEstimators;

namespace EstimatorComparison;

public record struct SimulationResults(ImmutableArray<double> HmmScores, ImmutableArray<double> Sts1Scores);

public record struct TTestResults(double MeanDiff, double StdDev, double TStat);

public static class RunSimulator
{
    public static SimulationResults Simulate(string filePath, int numPasses)
    {
        List<Floor> route = RunParser.Parse(filePath);
        
        double[] hmmScores = new double[numPasses];
        double[] sts1Scores = new double[numPasses];

        Parallel.For(0, numPasses, passIndex =>
        {
            int seed = filePath.GetHashCode() ^ passIndex;
            (double hmmScore, double sts1Score) = SimulateRun(route, seed);
            hmmScores[passIndex] = hmmScore;
            sts1Scores[passIndex] = sts1Score;
        });

        return new SimulationResults([.. hmmScores], [.. sts1Scores]);
    }

    private static (double, double) SimulateRun(List<Floor> route, int seed)
    {
        var potionSimulator = new PotionSimulator(seed);
        var hmmEstimator = new HmmEstimator();
        var sts1Estimator = new Sts1Estimator();

        int numCombats = 0;
        double hmmBrierSum = 0;
        double sts1BrierSum = 0;
        foreach (Floor floor in route)
        {
            if (!floor.IsCombat) continue;

            numCombats++;
            float eliteBonus = floor.IsElite ? 0.125f : 0f;

            float hmmPrediction = hmmEstimator.GetExpectedChance();
            float sts1Prediction = sts1Estimator.GetExpectedChance();

            float hmmEffective = Math.Clamp(hmmPrediction + eliteBonus, 0f, 1f);
            float sts1Effective = Math.Clamp(sts1Prediction + eliteBonus, 0f, 1f);

            bool dropped = potionSimulator.Roll(floor.IsElite);
            float outcome = dropped ? 1f : 0f;

            hmmBrierSum += Math.Pow(hmmEffective - outcome, 2);
            sts1BrierSum += Math.Pow(sts1Effective - outcome, 2);

            hmmEstimator.UpdateBelief(dropped, floor.IsElite);
            sts1Estimator.UpdateBelief(dropped, floor.IsElite);
        }

        double hmmBrier = hmmBrierSum / numCombats;
        double sts1Brier = sts1BrierSum / numCombats;
        
        return (hmmBrier, sts1Brier);
    }
    
    public static TTestResults PairedTTest(IReadOnlyList<double> hmmScores, IReadOnlyList<double> sts1Scores)
    {
        int n = hmmScores.Count;

        double totalDiff = 0;
        for (int i = 0; i < n; i++)
            totalDiff += (hmmScores[i] - sts1Scores[i]);
        double meanDiff = totalDiff / n;
        
        double sumOfSqdDev = 0;
        for (int i = 0; i < n; i++)
        {
            double residual = (hmmScores[i] - sts1Scores[i]) - meanDiff;
            sumOfSqdDev += residual * residual;
        }
        double stdDev = Math.Sqrt(sumOfSqdDev / (n - 1));
        
        double stdErr = stdDev / Math.Sqrt(n);
        double tStat = meanDiff / stdErr;

        return new TTestResults(meanDiff, stdDev, tStat);
    }
}