using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetricsAnalyzers.Metrics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetricsAnalyzers.Analyzers
{
    interface IAnalyzer
    {
        Task Calculate(BlockSyntax blockSyntax);
        Metrics.Metric Metric { get; }
    }
     abstract class Analyzer<T> : CSharpSyntaxWalker, IAnalyzer where T:Metrics.Metric
    {
        public Metrics.Metric Metric { get; protected set; }
        public abstract Task Calculate(BlockSyntax blockSyntax);

        Task IAnalyzer.Calculate(BlockSyntax blockSyntax)
        {
            return Calculate(blockSyntax);
        }
    }
}
