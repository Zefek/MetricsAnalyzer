using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsAnalyzers.Metrics
{
    public class Metric
    {
        protected Metric(int value)
        {
            Value = value;
        }
        public int Value { get; private set; }
        public Severity Severity { get; set; }
    }
    #region Metrics
    public class MaintainabilityMetric : Metric
    {
        internal MaintainabilityMetric(int value) : base(value) { }
    }
    public class CyclomaticComplexityMetric : Metric
    {
        internal CyclomaticComplexityMetric(int value) : base(value) { }
    }
    public class ClassCouplingMetric : Metric
    {
        internal ClassCouplingMetric(int value) : base(value) { }
    }
    public class LinesOfCodeMetric : Metric
    {
        internal LinesOfCodeMetric(int value) : base(value) { }
    }
    class LogicalComplexityMetric:Metric
    {
        internal LogicalComplexityMetric(int value) : base(value) { }
    }
    public class NumberOfParametersMetric : Metric
    {
        internal NumberOfParametersMetric(int value) : base(value) { }
    }
    public class NumberOfLocalVariablesMetric : Metric
    {
        internal NumberOfLocalVariablesMetric(int value) : base(value) { }
    }
    public class LinesOfCommentMetric : Metric
    {
        internal LinesOfCommentMetric(int value) : base(value) { }
    }
    #endregion
}
