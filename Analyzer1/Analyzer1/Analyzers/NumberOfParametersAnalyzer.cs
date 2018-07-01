using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetricsAnalyzers.Analyzers
{
    class NumberOfParametersAnalyzer : Analyzer<Metrics.NumberOfParametersMetric>
    {
        static object lockObject = new object();
        int counter = 0;
        public override Task Calculate(BlockSyntax blockSyntax)
        {
            return Task.Run(() =>
            {
                if (Metric == null)
                {
                    lock (lockObject)
                    {
                        if (Metric == null)
                        {
                            this.Visit(blockSyntax);
                            Metric = new Metrics.NumberOfParametersMetric(counter);
                        }
                    }
                }
            });
        }
        public override void VisitParameter(ParameterSyntax node)
        {
            base.VisitParameter(node);
            counter++;
        }
    }
}
