using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetricsAnalyzers.Analyzers
{
    class MaintainabilityIndexAnalyzer : Analyzer<Metrics.MaintainabilityMetric>
    {
        LinesOfCodeAnalyzer _linesOfCodeAnalyzer;
        CyclomaticComplexityAnalyzer _cyclomaticComplexityAnalyzer;
        HalsteadAnalyzer _halsteadAnalyzer;
        object lockObject = new object();

        public MaintainabilityIndexAnalyzer(LinesOfCodeAnalyzer linesOfCodeAnalyzer, CyclomaticComplexityAnalyzer cyclomaticComplexityAnalyzer)
        {
            _linesOfCodeAnalyzer = linesOfCodeAnalyzer;
            _cyclomaticComplexityAnalyzer = cyclomaticComplexityAnalyzer;
            _halsteadAnalyzer = new HalsteadAnalyzer();
        }
        public override Task Calculate(BlockSyntax blockSyntax)
        {
            return Task<Metrics.MaintainabilityMetric>.Run(() =>
            {
                if (Metric == null)
                {
                    lock (lockObject)
                    {
                        if (Metric == null)
                        {
                            Task linesOfCodeTask = _linesOfCodeAnalyzer.Calculate(blockSyntax);
                            Task cyclomaticComplexityTask = _cyclomaticComplexityAnalyzer.Calculate(blockSyntax);
                            Task halsteadTask = _halsteadAnalyzer.Calculate(blockSyntax);

                            Task.WaitAll(linesOfCodeTask, cyclomaticComplexityTask, halsteadTask);
                            Metric = new Metrics.MaintainabilityMetric(CalculateInternal(_linesOfCodeAnalyzer.Metric.Value, (_halsteadAnalyzer.Metric as Metrics.HalsteadMetrics).GetVolume(), _cyclomaticComplexityAnalyzer.Metric.Value));
                        }
                    }
                }
                return Metric;
            });
        }

        private int CalculateInternal(int linesOfCode, double? volume, int cyclomaticComplexity)
        {
            double num1 = 1;
            if (linesOfCode == 0)
            {
                num1 = 100;
            }
            else
            {
                double num = 1;
                if (volume.HasValue)
                {
                    num = Math.Log(volume.Value);
                }
                num1 = (171 - 5.2 * num - 0.23 * cyclomaticComplexity - 16.2 * Math.Log(linesOfCode)) * 100 / 171;
            }
            return (int)Math.Ceiling(num1);
        }
    }
}
