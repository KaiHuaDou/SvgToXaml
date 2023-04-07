using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SvgToXaml.Explorer
{
    /// <summary>
    ///     Interaction logic for FolderTree.xaml
    /// </summary>
    public partial class FolderTree
    {
        public static readonly DependencyProperty CurrentFolderProperty = DependencyProperty.Register(
            "CurrentFolder", typeof(string), typeof(FolderTree), new PropertyMetadata(default(string), CurrentFolderChanged));

        private static void CurrentFolderChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            FolderTree folderTree = (FolderTree) dependencyObject;
            TreeViewItem item = FindItem(folderTree.FoldersTree, (string) dependencyPropertyChangedEventArgs.NewValue);
            if (item != null)
                SelectItem(item);
        }

        public string CurrentFolder
        {
            get => (string) GetValue(CurrentFolderProperty);
            set => SetValue(CurrentFolderProperty, value);
        }

        private readonly object _dummyNode;

        public FolderTree( )
        {
            InitializeComponent( );
            FillRootLevel( );
            FoldersTree.SelectedItemChanged += FoldersTreeOnSelectedItemChanged;
        }

        public static readonly DependencyProperty ContextMenuCommandsProperty = DependencyProperty.Register(
            "ContextMenuCommands", typeof(ObservableCollection<Tuple<object, ICommand>>), typeof(FolderTree), new PropertyMetadata(default(ObservableCollection<Tuple<object, ICommand>>)));

        public ObservableCollection<Tuple<object, ICommand>> ContextMenuCommands
        {
            get => (ObservableCollection<Tuple<object, ICommand>>) GetValue(ContextMenuCommandsProperty);
            set => SetValue(ContextMenuCommandsProperty, value);
        }

        private void FoldersTreeOnSelectedItemChanged(object o, RoutedPropertyChangedEventArgs<object> routedPropertyChangedEventArgs)
        {
            if (routedPropertyChangedEventArgs.NewValue is TreeViewItem)
                CurrentFolder = (string) (routedPropertyChangedEventArgs.NewValue as TreeViewItem).Tag;
        }

        private void FillRootLevel( )
        {
            foreach (string drive in Directory.GetLogicalDrives( ))
            {
                TreeViewItem item = new TreeViewItem
                {
                    Header = drive,
                    Tag = drive
                };
                item.Items.Add(_dummyNode);
                item.Expanded += FolderExpanded;

                // Apply the attached property so that 
                // the triggers know that this is root item.
                TreeViewItemProps.SetIsRootLevel(item, true);

                FoldersTree.Items.Add(item);
            }
        }

        private void FolderExpanded(object o, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem) o;
            if (item.Items.Count == 1 && item.Items[0] == _dummyNode)
            {
                item.Items.Clear( );
                try
                {
                    if (item.Tag == null) return;
                    foreach (string dir in Directory.GetDirectories((string) item.Tag))
                    {
                        TreeViewItem subitem = new TreeViewItem
                        {
                            Header = new DirectoryInfo(dir).Name,
                            Tag = dir
                        };
                        subitem.Items.Add(_dummyNode);
                        subitem.Expanded += FolderExpanded;
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public static void SelectItem(TreeViewItem item)
        {
            item.Focus( );
            item.IsSelected = true;
            item.BringIntoView( );
        }

        public static TreeViewItem FindItem(TreeView tv, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            IEnumerable<string> parts = GetDirParts(path);
            ItemCollection currItems = tv.Items;
            TreeViewItem found = null;
            foreach (string part in parts)
            {
                TreeViewItem newFound = currItems.Cast<TreeViewItem>( )
                    .FirstOrDefault(
                        e => string.Equals(e.Header.ToString( ), part, StringComparison.OrdinalIgnoreCase));
                if (newFound != null)
                {
                    found = newFound;
                    found.IsExpanded = true;
                    currItems = found.Items;
                }
            }
            return found;
        }

        private static IEnumerable<string> GetDirParts(string path)
        {
            path = Path.GetFullPath(path);
            //yield return Path.GetPathRoot(path);
            string[] parts = path.Split(Path.DirectorySeparatorChar).ToArray( );
            if (parts.Length > 0 && parts[0].Length == 2)
                parts[0] += @"\";
            return parts;
        }

        public event RoutedPropertyChangedEventHandler<object> SelectedItemChanged
        {
            add => FoldersTree.SelectedItemChanged += value;
            remove => FoldersTree.SelectedItemChanged -= value;
        }

    }
}
