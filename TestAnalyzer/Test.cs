using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetricsAnalyzers;
using MetricsAnalyzers.Metrics;

namespace DefaultAnalyzer
{
    public class DefaultAnalyzer : MetricsAnalyzers.MetricAnalyzer
    {
        List<Analyzer> analyzers = new List<Analyzer>();
        public override void Initialize(MetricsContext context)
        {
            analyzers.Add(new LoCAnalyzer());
            analyzers.Add(new CCAnalyzer());
            analyzers.Add(new MTAnalyzer());
            analyzers.Add(new LinesOfCodeAnalyzer());
            context.RegisterCalculated(Calculated);
        }

        private void Calculated(Node obj)
        {
            foreach (Analyzer a in analyzers)
                a.Evaluate(obj.Metrics);
        }
    }
    abstract class Analyzer
    {
        public abstract void Evaluate(IEnumerable<Metric> metrics);
    }
    class LoCAnalyzer : Analyzer
    {
        public override void Evaluate(IEnumerable<Metric> metrics)
        {
            LinesOfCodeMetric loc = metrics.SingleOrDefault(k => k is LinesOfCodeMetric) as LinesOfCodeMetric;
            CyclomaticComplexityMetric cc = metrics.SingleOrDefault(k => k is CyclomaticComplexityMetric) as CyclomaticComplexityMetric;

            if (loc == null)
                return;
            Severity result = Severity.Hidden;
            if (loc.Value >= 25 && loc.Value < 35)
                result = Severity.Warning;
            if (loc.Value >= 35)
                result = Severity.Error;
            if ((result == Severity.Warning || result == Severity.Error) && cc != null && cc.Value <= 3)
                result = Severity.Hidden;
            loc.Severity = result;
        }
    }
    class CCAnalyzer : Analyzer
    {
        public override void Evaluate(IEnumerable<Metric> metrics)
        {
            CyclomaticComplexityMetric cc = metrics.SingleOrDefault(k => k is CyclomaticComplexityMetric) as CyclomaticComplexityMetric;
            if (cc == null)
                return;
            if (cc.Value >= 10)
                cc.Severity = Severity.Warning;
            if (cc.Value >= 15)
                cc.Severity = Severity.Error;
        }
    }
    class MTAnalyzer : Analyzer
    {
        public override void Evaluate(IEnumerable<Metric> metrics)
        {
            MaintainabilityMetric mt = metrics.SingleOrDefault(k => k is MaintainabilityMetric) as MaintainabilityMetric;
            if (mt == null)
                return;
            if (mt.Value >= 20)
                mt.Severity = Severity.Hidden;
            if (mt.Value >= 10 && mt.Value < 20)
                mt.Severity = Severity.Warning;
            if (mt.Value < 10)
                mt.Severity = Severity.Error;
        }
    }
    class LinesOfCodeAnalyzer : Analyzer
    {
        public override void Evaluate(IEnumerable<Metric> metrics)
        {
            LinesOfCodeMetric loc = metrics.SingleOrDefault(k => k is LinesOfCodeMetric) as LinesOfCodeMetric;
            if (loc == null)
                return;
            loc.Severity = Severity.Hidden;
            if (loc.Value == 0)
                loc.Severity = Severity.Warning;
        }
    }
}
