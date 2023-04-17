using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SvgToXaml.ViewModels;

internal sealed class GraphicImageViewModel : ImageBaseViewModel
{
    public GraphicImageViewModel(string filePath) : base(filePath) { }
    protected override ImageSource GetImageSource( )
    {
        return new BitmapImage(new Uri(FilePath, UriKind.RelativeOrAbsolute));
    }

    public static string SupportedFormats => "*.jpg|*.jpeg|*.png|*.bmp|*.tiff|*.gif";

    protected override string GetSvgDesignInfo( )
    {
        return PreviewSource is BitmapImage bi ? $"{bi.PixelWidth}x{bi.PixelHeight}" : null;
    }

    public override bool HasXaml => false;
    public override bool HasSvg => false;
}
