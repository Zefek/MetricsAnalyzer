using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetricsAnalyzers;

namespace Plugins
{
    public class Plugin : IPlugins
    {
        public IEnumerable<MetricAnalyzer> GetAnalyzers()
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string[] files = System.IO.Directory.GetFiles(fi.Directory.FullName, "*.dll");
            List<MetricAnalyzer> result = new List<MetricAnalyzer>();
            foreach (string file in files)
            {
                try
                {
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFrom(file);
                    foreach (Type t in assembly.ExportedTypes)
                    {
                        if (t.BaseType == typeof(MetricsAnalyzers.MetricAnalyzer))
                        {
                            result.Add((MetricAnalyzer)Activator.CreateInstance(t));
                        }
                    }

                }
                catch(Exception)
                {
                    //This block needs to be empty
                    //because of result could be empty
                    //logging is not necessary.
                }
            }
            return result;
        }
    }
}
