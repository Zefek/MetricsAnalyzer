using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetricsAnalyzers.Analyzers
{
    class CyclomaticComplexityAnalyzer : Analyzer<Metrics.CyclomaticComplexityMetric>
    {
        LogicalComplexityAnalyzer logicalComplexityAnalyzer;
        int counter = 0;
        object lockObject = new object();

        public CyclomaticComplexityAnalyzer()
        {
            logicalComplexityAnalyzer = new LogicalComplexityAnalyzer();
        }
        public override Task Calculate(BlockSyntax blockSyntax)
        {
            return Task.Run(() =>
            {
                if (this.Metric == null)
                {
                    lock (lockObject)
                    {
                        if (Metric == null)
                        {
                            this.Visit(blockSyntax);
                            Task.WaitAll(logicalComplexityAnalyzer.Calculate(blockSyntax));
                            int result = counter + logicalComplexityAnalyzer.Metric.Value;
                            Metric = new Metrics.CyclomaticComplexityMetric(result == 0 ? 1 : result);
                        }
                    }
                }
            });
        }
        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            base.VisitBinaryExpression(node);
            if (node.OperatorToken.Kind() == SyntaxKind.QuestionQuestionToken)
            {
                counter++;
            }
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            base.VisitConditionalExpression(node);
            if (node.QuestionToken.Kind() == SyntaxKind.QuestionToken && node.ColonToken.Kind() == SyntaxKind.ColonToken)
            {   
                counter++;
            }
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            base.VisitDoStatement(node);
            counter++;
        }

        public override void VisitElseClause(ElseClauseSyntax node)
        {
            base.VisitElseClause(node);
            counter++;
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            base.VisitForEachStatement(node);
            counter++;
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            base.VisitForStatement(node);
            counter++;
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            base.VisitIfStatement(node);
            counter++;
        }

        public override void VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            base.VisitInitializerExpression(node);
            counter++;
        }

        public override void VisitSwitchSection(SwitchSectionSyntax node)
        {
            base.VisitSwitchSection(node);
            counter++;
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            base.VisitWhileStatement(node);
            counter++;
        }
    }
}
