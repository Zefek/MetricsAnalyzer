using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetricsAnalyzers.Metrics;

namespace MetricsAnalyzers
{
    public abstract class MetricAnalyzer
    {
        public abstract void Initialize(MetricsContext context);
    }    
}
