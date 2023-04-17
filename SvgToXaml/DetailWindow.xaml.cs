using System;
using System.Collections.Generic;
using System.Linq;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
//using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using SvgToXaml.ViewModels;

namespace SvgToXaml;

/// <summary>
/// Folding doesn't work -> keep the code
/// </summary>
public partial class DetailWindow
{
    //private XmlFoldingStrategy xamlFoldings;
    //private FoldingManager xamlFoldingMgr;
    //private volatile bool xamlUpdating;

    //private XmlFoldingStrategy svgFoldings;
    //private FoldingManager svgFoldingMgr;
    //private volatile bool svgUpdating;

    public DetailWindow( )
    {
        InitializeComponent( );
    }

    private void WindowLoaded(object o, RoutedEventArgs e)
    {
        if (DataContext is not SvgImageViewModel context)
            return;
        if (context.HasXaml)
        {
            XamlViewer.Document.Text = context.Xaml;
            XamlViewer.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
            //xamlFoldingMgr = FoldingManager.Install(XamlViewer.TextArea);
            //xamlFoldings = new( );
            //XamlViewer.Document.TextChanged += XamlChanged;
            //xamlFoldings.UpdateFoldings(xamlFoldingMgr, XamlViewer.Document);
        }
        if (context.HasSvg)
        {
            SvgViewer.Document.Text = context.Svg;
            SvgViewer.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
            //svgFoldingMgr = FoldingManager.Install(SvgViewer.TextArea);
            //svgFoldings = new( );
            //SvgViewer.Document.TextChanged += SvgChanged;
            //svgFoldings.UpdateFoldings(svgFoldingMgr, SvgViewer.Document);
        }
    }
    private void CopyToClipboardClick(object o, RoutedEventArgs e)
    {
        Clipboard.SetText(XamlViewer.Text);
    }

    private void ToggleStretchClicked(object o, MouseButtonEventArgs e)
    {
        List<Stretch> values = Enum.GetValues(typeof(Stretch)).OfType<Stretch>( ).ToList( );
        int idx = values.IndexOf(Image.Stretch);
        idx = (idx + 1) % values.Count;
        Image.Stretch = values[idx];
    }

    //private async void XamlChanged(object o, EventArgs e)
    //{
    //    if (!xamlUpdating)
    //    {
    //        xamlUpdating = true;
    //        await Task.Delay(1000);
    //    }
    //    xamlUpdating = false;
    //    xamlFoldings.UpdateFoldings(xamlFoldingMgr, XamlViewer.Document);
    //}

    //private async void SvgChanged(object o, EventArgs e)
    //{
    //    if (!svgUpdating)
    //    {
    //        svgUpdating = true;
    //        await Task.Delay(1000);
    //    }
    //    svgUpdating = false;
    //    svgFoldings.UpdateFoldings(svgFoldingMgr, SvgViewer.Document);
    //}
}
