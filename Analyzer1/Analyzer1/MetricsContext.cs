using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsAnalyzers
{
    public class MetricsContext
    {
        List<Action<Node>> nodeActions = new List<Action<Node>>();
        public void RegisterCalculated(Action<Node> node)
        {
            nodeActions.Add(node);
        }
        internal void Invoke(Node node)
        {
            foreach (Action<Node> n in nodeActions)
                n.Invoke(node);
        }
    }
}
