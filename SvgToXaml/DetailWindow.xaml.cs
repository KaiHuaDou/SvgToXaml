using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SvgToXaml;

public partial class DetailWindow
{
    public DetailWindow( )
    {
        InitializeComponent( );
    }

    private void CopyToClipboardClick(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(XamlViewer.Text);
    }

    private void ToggleStretchClicked(object sender, MouseButtonEventArgs e)
    {
        List<Stretch> values = Enum.GetValues(typeof(Stretch)).OfType<Stretch>( ).ToList( );
        int idx = values.IndexOf(Image.Stretch);
        idx = (idx + 1) % values.Count;
        Image.Stretch = values[idx];
    }
}
