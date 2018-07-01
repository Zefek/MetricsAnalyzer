using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MetricsAnalyzers
{
    public interface IPlugins
    {
        IEnumerable<MetricAnalyzer> GetAnalyzers();
    }
    class Plugins
    {
        public static IEnumerable<MetricAnalyzer> Load()
        {
            
            System.Reflection.AssemblyName an = new System.Reflection.AssemblyName("Plugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            foreach (Type t in System.Reflection.Assembly.Load(an).ExportedTypes)
            {
                Type pluginType = t.GetTypeInfo().ImplementedInterfaces.SingleOrDefault(k => k == typeof(IPlugins));
                IPlugins result = (IPlugins)Activator.CreateInstance(t);
                return result.GetAnalyzers();
            }
            return null;
        }
    }
}
