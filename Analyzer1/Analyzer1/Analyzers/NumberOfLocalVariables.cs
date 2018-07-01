using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetricsAnalyzers.Analyzers
{
    class NumberOfLocalVariablesAnalyzer : Analyzer<Metrics.NumberOfLocalVariablesMetric>
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
                            Metric = new Metrics.NumberOfLocalVariablesMetric(counter);
                        }
                    }
                }
            });
        }
        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            base.VisitVariableDeclaration(node);
            counter++;
        }
    }
}
