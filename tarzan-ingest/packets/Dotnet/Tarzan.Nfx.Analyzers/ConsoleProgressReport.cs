using System;

namespace Tarzan.Nfx.Analyzers
{ 
        public class ConsoleProgressReport : IProgress<FlowAnalyzer.ProgressRecord>
        {
            public ConsoleProgressReport()
            {
            }
            public void Report(FlowAnalyzer.ProgressRecord value)
            {
                Console.WriteLine($"\rProgress: Frames={value.CompletedFrames}/{value.TotalFrames}, Flows={value.CompletedFlows}/{value.TotalFlows}, Elapsed={value.ElapsedTime.ElapsedMilliseconds}ms.");
            }
        }
}
