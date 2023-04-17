using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using SvgToXaml.Command;

namespace SvgToXaml.ViewModels;

public abstract class ImageBaseViewModel : ViewModelBase
{
    protected ImageBaseViewModel(string filePath)
    {
        FilePath = filePath;
        OpenDetailCommand = new DelegateCommand(OpenDetailExecute);
        OpenFileCommand = new DelegateCommand(OpenFileExecute);
    }

    public string FilePath { get; }
    public string FileName => Path.GetFileName(FilePath);
    public ImageSource PreviewSource => GetImageSource( );
    public ICommand OpenDetailCommand { get; set; }
    public ICommand OpenFileCommand { get; set; }
    protected abstract ImageSource GetImageSource( );
    public abstract bool HasXaml { get; }
    public abstract bool HasSvg { get; }
    public string SvgDesignInfo => GetSvgDesignInfo( );

    private void OpenDetailExecute( )
    {
        OpenDetailWindow(this);
    }

    public static void OpenDetailWindow(ImageBaseViewModel imageBaseViewModel)
    {
        new DetailWindow { DataContext = imageBaseViewModel }.Show( );
    }

    private void OpenFileExecute( )
    {
        Process.Start(FilePath);
    }

    protected abstract string GetSvgDesignInfo( );
}
