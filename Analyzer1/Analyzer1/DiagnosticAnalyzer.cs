using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MetricsAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MetricsAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    MaintainabilityIndexDiagnosticsAnalyzer.Rule,
                    CyclomaticComplexityDiagnosticsAnalyzer.Rule,
                    LinesOfCodeDiagnosticsAnalyzer.Rule,
                    NumberOfParametersAnalyzer.Rule,
                    NumberOfLocalVariablesAnalyzer.Rule,
                    LinesOfCommentAnalyzer.Rule
                    );
            }
        }

        MetricsContext context = new MetricsContext();
        List<MetricAnalyzer> analyzers = new List<MetricAnalyzer>();
        List<MetricDiagnosticsAnalyzer> diagnosticsAnalyzers = new List<MetricDiagnosticsAnalyzer>();
        public override void Initialize(AnalysisContext context)
        {
            InitializeAnalyzers();
            context.RegisterCodeBlockAction(CodeBlock);
        }
        private void InitializeAnalyzers()
        {
            diagnosticsAnalyzers.Add(new MaintainabilityIndexDiagnosticsAnalyzer());
            diagnosticsAnalyzers.Add(new CyclomaticComplexityDiagnosticsAnalyzer());
            diagnosticsAnalyzers.Add(new LinesOfCodeDiagnosticsAnalyzer());
            diagnosticsAnalyzers.Add(new NumberOfParametersAnalyzer());
            diagnosticsAnalyzers.Add(new NumberOfLocalVariablesAnalyzer());
            diagnosticsAnalyzers.Add(new LinesOfCommentAnalyzer());
            LoadAnalyzers();            
        }
        private void LoadAnalyzers()
        {
            foreach (MetricAnalyzer analyzer in Plugins.Load())
            {
                analyzer.Initialize(context);
                analyzers.Add(analyzer);
            }
        }
        private void CodeBlock(CodeBlockAnalysisContext obj)
        {
            if(analyzers.Count==0)
            {
                LoadAnalyzers();
            }
            MemberNode node = new MemberNode(context);
            node.Calculate(obj.CodeBlock);

            foreach(Metrics.Metric metric in node.Metrics)
            {
                foreach(MetricDiagnosticsAnalyzer analyzer in diagnosticsAnalyzers)
                {
                    if (analyzer.IsMatch(metric))
                    {
                        analyzer.ReportDiagnotics(obj, node, metric);
                    }
                }
            }
        }
        
    }
    #region Analyzers
    abstract class MetricDiagnosticsAnalyzer
    {
        protected abstract DiagnosticDescriptor GetDescriptor(Metrics.Metric metric, string name);
        protected abstract Type MetricType { get; }
        public bool IsMatch(Metrics.Metric metric)
        {
            return metric.GetType() == MetricType;
        }
        public void ReportDiagnotics(CodeBlockAnalysisContext codeBlockAnalysisContext, MemberNode memberNode, Metrics.Metric metric)
        {
            var diagnostics = Diagnostic.Create(GetDescriptor(metric, memberNode.Name), memberNode.Location, memberNode.Name);
            codeBlockAnalysisContext.ReportDiagnostic(diagnostics);
        }
        protected DiagnosticSeverity GetSeverity(Severity severity)
        {
            switch (severity)
            {
                case Severity.Hidden: return DiagnosticSeverity.Hidden;
                case Severity.Error: return DiagnosticSeverity.Error;
                case Severity.Info: return DiagnosticSeverity.Info;
                case Severity.Warning: return DiagnosticSeverity.Warning;
            }
            return default(DiagnosticSeverity);
        }
    }
    class MaintainabilityIndexDiagnosticsAnalyzer : MetricDiagnosticsAnalyzer
    {
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.MaintainabilityIndexAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.MaintainabilityIndexAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.MaintainabilityIndexAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        public const string DiagnosticId = "MS001";
        private const string Category = "Metrics";
        public static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Hidden, isEnabledByDefault: true, description: Description);

        protected override Type MetricType { get { return typeof(Metrics.MaintainabilityMetric); } }

        protected override DiagnosticDescriptor GetDescriptor(Metrics.Metric metric, string name)
        {
            return new DiagnosticDescriptor(DiagnosticId, Title, string.Format(MessageFormat.ToString(), name, metric.Value), Category, GetSeverity(metric.Severity), isEnabledByDefault: true, description: Description);
        }
    }
    class CyclomaticComplexityDiagnosticsAnalyzer : MetricDiagnosticsAnalyzer
    {
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.CyclomaticComplexityAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.CyclomaticComplexityAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.CyclomaticComplexityAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        public const string DiagnosticId = "MS002";
        private const string Category = "Metrics";
        public static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Hidden, isEnabledByDefault: true, description: Description);

        protected override Type MetricType { get { return typeof(Metrics.CyclomaticComplexityMetric); } }
        protected override DiagnosticDescriptor GetDescriptor(Metrics.Metric metric, string name)
        {
            return new DiagnosticDescriptor(DiagnosticId, Title, string.Format(MessageFormat.ToString(), name, metric.Value), Category, GetSeverity(metric.Severity), isEnabledByDefault: true, description: Description);
        }
    }
    class LinesOfCodeDiagnosticsAnalyzer : MetricDiagnosticsAnalyzer
    {
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.LinesOfCodeAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.LinesOfCodeAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.LinesOfCodeAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        public const string DiagnosticId = "MS003";
        private const string Category = "Metrics";
        public static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Hidden, isEnabledByDefault: true, description: Description);

        protected override Type MetricType { get { return typeof(Metrics.LinesOfCodeMetric); } }
        protected override DiagnosticDescriptor GetDescriptor(Metrics.Metric metric, string name)
        {
            return new DiagnosticDescriptor(DiagnosticId, Title, string.Format(MessageFormat.ToString(), name, metric.Value), Category, GetSeverity(metric.Severity), isEnabledByDefault: true, description: Description);
        }
    }
    class NumberOfParametersAnalyzer : MetricDiagnosticsAnalyzer
    {
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.NumberOfParametersAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.NumberOfParametersAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.NumberOfParametersAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        public const string DiagnosticId = "MS004";
        private const string Category = "Metrics";
        public static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Hidden, isEnabledByDefault: true, description: Description);

        protected override Type MetricType { get { return typeof(Metrics.NumberOfParametersMetric); } }
        protected override DiagnosticDescriptor GetDescriptor(Metrics.Metric metric, string name)
        {
            return new DiagnosticDescriptor(DiagnosticId, Title, string.Format(MessageFormat.ToString(), name, metric.Value), Category, GetSeverity(metric.Severity), isEnabledByDefault: true, description: Description);
        }
    }
    class NumberOfLocalVariablesAnalyzer : MetricDiagnosticsAnalyzer
    {
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.NumberOfLocalVariablesAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.NumberOfLocalVariablesAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.NumberOfLocalVariablesAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        public const string DiagnosticId = "MS005";
        private const string Category = "Metrics";
        public static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Hidden, isEnabledByDefault: true, description: Description);

        protected override Type MetricType { get { return typeof(Metrics.NumberOfLocalVariablesMetric); } }
        protected override DiagnosticDescriptor GetDescriptor(Metrics.Metric metric, string name)
        {
            return new DiagnosticDescriptor(DiagnosticId, Title, string.Format(MessageFormat.ToString(), name, metric.Value), Category, GetSeverity(metric.Severity), isEnabledByDefault: true, description: Description);
        }
    }
    class LinesOfCommentAnalyzer : MetricDiagnosticsAnalyzer
    {
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.LinesOfCommentAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.LinesOfCommentAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.LinesOfCommentAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        public const string DiagnosticId = "MS006";
        private const string Category = "Metrics";
        public static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Hidden, isEnabledByDefault: true, description: Description);

        protected override Type MetricType { get { return typeof(Metrics.LinesOfCommentMetric); } }
        protected override DiagnosticDescriptor GetDescriptor(Metrics.Metric metric, string name)
        {
            return new DiagnosticDescriptor(DiagnosticId, Title, string.Format(MessageFormat.ToString(), name, metric.Value), Category, GetSeverity(metric.Severity), isEnabledByDefault: true, description: Description);
        }
    }
    #endregion
}
