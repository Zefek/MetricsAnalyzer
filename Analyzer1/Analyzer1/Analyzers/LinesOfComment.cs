using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MetricsAnalyzers.Analyzers
{
    class LinesOfCommentAnalyzer : Analyzer<Metrics.LinesOfCommentMetric>
    {
        static object lockObject = new object();
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
                            Metric = new Metrics.LinesOfCommentMetric(GetLinesOfComment(blockSyntax));
                        }
                    }
                }
            });
        }
        private int GetLinesOfComment(BlockSyntax blockSyntax)
        {
            IEnumerable<SyntaxTrivia> result = blockSyntax.DescendantTrivia(null, false);
            result = result.Where((SyntaxTrivia node) =>
            {
                return node.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.MultiLineCommentTrivia)
                    || node.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SingleLineCommentTrivia);
            });
            int lines = 0;
            foreach(SyntaxTrivia syntaxTrivia in result)
            {
                FileLinePositionSpan span = syntaxTrivia.GetLocation().GetLineSpan();
                lines += span.EndLinePosition.Line - span.StartLinePosition.Line + 1;
            }
            return lines;
        }
    }
}
