using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetricsAnalyzers.Analyzers
{
    class LogicalComplexityAnalyzer : Analyzer<Metrics.LogicalComplexityMetric>
    {
        int counter = 0;
        public override Task Calculate(BlockSyntax blockSyntax)
        {
            return Task.Run(() =>
            {
                this.Visit(blockSyntax);
                this.Metric = new Metrics.LogicalComplexityMetric(counter);
            });
        }
        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            base.VisitBinaryExpression(node);
            SyntaxKind syntaxKind = node.Kind();
            switch (syntaxKind)
            {
                case SyntaxKind.LogicalOrExpression:
                case SyntaxKind.LogicalAndExpression:
                    {
                        counter++;
                        break;
                    }
                case SyntaxKind.BitwiseOrExpression:
                case SyntaxKind.BitwiseAndExpression:
                case SyntaxKind.ExclusiveOrExpression:
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                    {
                        break;
                    }
                default:
                    {
                        if (syntaxKind == SyntaxKind.LogicalNotExpression)
                        {
                            counter++;
                        }
                        break;
                    }
            }
        }
    }
}
