using System.IO;
using FluentAssertions;
using NUnit.Framework;
using SvgConverter;

namespace SvgConverterTest;

[TestFixture]
public class CommandLineTests
{
    [Test]
    public void EmptyArgsTest1( )
    {
        string arg = null;
        CmdLineHandler.HandleCommandLine(arg).Should( ).Be(0);
    }
    [Test]
    public void EmptyArgsTest2( )
    {
        CmdLineHandler.HandleCommandLine("").Should( ).NotBe(0);
    }
    [Test]
    public void HelpTest( )
    {
        CmdLineHandler.HandleCommandLine("-H").Should( ).Be(0);
    }

    [Test]
    public void DirTest( )
    {
        string resultFile = ".\\images.xaml";
        if (File.Exists(resultFile))
            File.Delete(resultFile);
        CmdLineHandler.HandleCommandLine("BuildDict /inputdir:\"..\\..\\TestFiles\\\" /outputname:images /outputdir:.").Should( ).Be(0);
        File.Exists(resultFile).Should( ).BeTrue( );
    }

    [Test]
    public void SubDirTest( )
    {
        string resultFile = ".\\images.xaml";
        if (File.Exists(resultFile))
            File.Delete(resultFile);
        CmdLineHandler.HandleCommandLine("BuildDict /inputdir:\"..\\..\\TestFiles\\Subfolder1\\\" /handleSubFolders:true /outputname:images /outputdir:.").Should( ).Be(0);
        File.Exists(resultFile).Should( ).BeTrue( );
    }
}
