using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetricsAnalyzers.Metrics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetricsAnalyzers.Analyzers
{
    class ClassCouplingAnalyzer : Analyzer<Metrics.ClassCouplingMetric>
    {
        int counter = 0;
        object lockObject = new object();
        public override Task Calculate(BlockSyntax blockSyntax)
        {
            return Task<Metrics.ClassCouplingMetric>.Run(() =>
            {
                if (Metric == null)
                {
                    lock (lockObject)
                    {
                        if (Metric == null)
                        {
                            Metric = new ClassCouplingMetric(counter);
                        }
                    }
                }
            });
        }
    }
}
