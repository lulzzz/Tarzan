using Microsoft.Extensions.CommandLineUtils;
using System;
using Tarzan.Nfx.ProtocolClassifiers.Commands;

namespace Tarzan.Nfx.ProtocolClassifiers
{
    class Program
    {
        static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(true);

            var trainCmd = new TrainCommand();
            commandLineApplication.Command(trainCmd.Name, trainCmd.ExecuteCommand);

            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.Name = typeof(Program).Assembly.GetName().Name;
            commandLineApplication.FullName = $"Tarzan.Nfx Protocol Classifier ({typeof(Program).Assembly.GetName().Version})";

            commandLineApplication.OnExecute(() =>
            {
                commandLineApplication.Error.WriteLine();
                commandLineApplication.ShowHelp();
                return -1;
            });
            try
            {
                commandLineApplication.Execute(args);
            }
            catch (CommandParsingException e)
            {
                commandLineApplication.Error.WriteLine($"ERROR: {e.Message}");
            }
            catch (ArgumentException e)
            {
                commandLineApplication.Error.WriteLine($"ERROR: {e.Message}");
            }
        }
    }
}
