using System;
using System.Threading.Tasks;
using System.Windows;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;

namespace SvgToXaml.TextViewer;

/// <summary>
/// The whole folding does not work properly -> keep the code
/// </summary>
public class XmlViewer : TextEditor
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        "Text", typeof(string), typeof(XmlViewer), new PropertyMetadata(default(string), TextChanged));

    private static new void TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        XmlViewer viewer = (XmlViewer) d;
        viewer.Document.Text = (string) e.NewValue;
    }

    public new string Text
    {
        get => Document.Text;
        set => SetValue(TextProperty, value);
    }

    public XmlViewer( )
    {
        SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
        Options.EnableHyperlinks = true;
        Options.EnableEmailHyperlinks = true;
        Options.AllowScrollBelowDocument = true;
        ShowLineNumbers = true;
        foldingManager = FoldingManager.Install(TextArea);
        foldingStrategy = new XmlFoldingStrategy( );
        Document.TextChanged += Document_TextChanged;
    }

    private async void Document_TextChanged(object o, EventArgs e)
    {
        if (!onWaitingUpdate)
        {
            onWaitingUpdate = true;
            await Task.Delay(1000);
        }
        onWaitingUpdate = false;
        foldingStrategy.UpdateFoldings(foldingManager, Document);
    }

    private XmlFoldingStrategy foldingStrategy;
    private FoldingManager foldingManager;
    private volatile bool onWaitingUpdate;
}
