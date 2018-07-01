using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsAnalyzers.Metrics
{
    class HalsteadMetrics : Metric
    {
        public HalsteadMetrics() : base(0) { }
        public int NumOperands { get; internal set; }
        public int NumOperators { get; internal set; }
        public int NumUniqueOperands { get; internal set; }
        public int NumUniqueOperators { get; internal set; }
        public double? GetBugs()
        {
            double? effort = this.GetEffort();
            if (!effort.HasValue)
            {
                return null;
            }
            return new double?((double)effort.GetValueOrDefault() / 3000);
        }

        public double GetDifficulty()
        {
            return (double)this.NumUniqueOperators / 2 * ((double)this.NumOperands / (double)this.NumUniqueOperands);
        }

        public double? GetEffort()
        {
            double difficulty = this.GetDifficulty();
            double? volume = this.GetVolume();
            if (!volume.HasValue)
            {
                return null;
            }
            double num = difficulty;
            double? nullable = volume;
            if (!nullable.HasValue)
            {
                return null;
            }
            return new double?(num * (double)nullable.GetValueOrDefault());
        }

        public int GetLength()
        {
            return checked(this.NumOperators + this.NumOperands);
        }

        public int GetVocabulary()
        {
            return checked(this.NumUniqueOperators + this.NumUniqueOperands);
        }

        public double? GetVolume()
        {
            double num = 2;
            double vocabulary = (double)this.GetVocabulary();
            double length = (double)this.GetLength();
            if (vocabulary == 0)
            {
                return null;
            }
            return new double?(length * Math.Log(vocabulary, num));
        }
    }
}
