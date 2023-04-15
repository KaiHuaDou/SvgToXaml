using System;
using BKLib.CommandLineParser;

namespace SvgConverter;

public static class CmdLineHandler
{
    public static int HandleCommandLine(string arg)
    {
        string[] args = arg != null ? arg.Split(' ') : new string[0];
        return HandleCommandLine(args);
    }
    public static int HandleCommandLine(string[] args)
    {
        CommandLineParser praser = new( )
        {
            SkipCommandsWhenHelpRequested = true,
            Target = new CmdLineTarget( ),
            Header = "SvgToXaml - Tool to convert SVGs to a Dictionary",
            LogErrorsToConsole = true
        };
        try
        {
            return praser.ParseArgs(args, true);
        }
        catch (Exception)
        {
            // The errors are hopefully already reported via CommandLineParser
            Console.WriteLine("Error while handling Commandline.");
            return -1;
        }
    }
}
