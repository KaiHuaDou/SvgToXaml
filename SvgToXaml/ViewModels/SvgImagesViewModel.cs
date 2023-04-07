﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using SvgConverter;
using SvgToXaml.Command;
using SvgToXaml.Infrastructure;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SvgToXaml.ViewModels
{
    public class SvgImagesViewModel : ViewModelBase, IDisposable
    {
        private string _currentDir;
        private ObservableCollectionSafe<ImageBaseViewModel> _images;
        private ImageBaseViewModel _selectedItem;

        public SvgImagesViewModel( )
        {
            _images = new ObservableCollectionSafe<ImageBaseViewModel>( );
            OpenFileCommand = new DelegateCommand(OpenFileExecute);
            OpenFolderCommand = new DelegateCommand(OpenFolderExecute);
            ExportDirCommand = new DelegateCommand(ExportDirExecute);
            InfoCommand = new DelegateCommand(InfoExecute);

            ContextMenuCommands = new ObservableCollection<Tuple<object, ICommand>>
            {
                new Tuple<object, ICommand>("Open Explorer", new DelegateCommand<string>(OpenExplorerExecute))
            };
        }

        private void OpenFolderExecute( )
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog { Description = "Open Folder", SelectedPath = CurrentDir, ShowNewFolderButton = false };
            if (folderDialog.ShowDialog( ) == DialogResult.OK)
                CurrentDir = folderDialog.SelectedPath;
        }

        private void OpenFileExecute( )
        {
            OpenFileDialog openDlg = new OpenFileDialog { CheckFileExists = true, Filter = "Svg-Files|*.svg*", Multiselect = false };
            if (openDlg.ShowDialog( ).GetValueOrDefault( ))
            {
                ImageBaseViewModel.OpenDetailWindow(new SvgImageViewModel(openDlg.FileName));
            }
        }

        private void ExportDirExecute( )
        {
            string outFileName = Path.GetFileNameWithoutExtension(CurrentDir) + ".xaml";
            SaveFileDialog saveDlg = new SaveFileDialog { AddExtension = true, DefaultExt = ".xaml", Filter = "Xaml-File|*.xaml", InitialDirectory = CurrentDir, FileName = outFileName };
            if (saveDlg.ShowDialog( ) == DialogResult.OK)
            {
                string namePrefix = null;

                bool useComponentResKeys = false;
                string nameSpaceName = null;
                string nameSpace = Microsoft.VisualBasic.Interaction.InputBox("Enter a NameSpace for using static ComponentResKeys (or leave empty to not use it)", "NameSpace");
                if (!string.IsNullOrWhiteSpace(nameSpace))
                {
                    useComponentResKeys = true;
                    nameSpaceName =
                        Microsoft.VisualBasic.Interaction.InputBox(
                            "Enter a Name of NameSpace for using static ComponentResKeys", "NamespaceName");
                }
                else
                {
                    namePrefix = Microsoft.VisualBasic.Interaction.InputBox("Enter a namePrefix (or leave empty to not use it)", "Name Prefix");
                    if (string.IsNullOrWhiteSpace(namePrefix))
                        namePrefix = null;

                }

                outFileName = Path.GetFullPath(saveDlg.FileName);
                ResKeyInfo resKeyInfo = new ResKeyInfo
                {
                    XamlName = Path.GetFileNameWithoutExtension(outFileName),
                    Prefix = namePrefix,
                    UseComponentResKeys = useComponentResKeys,
                    NameSpace = nameSpace,
                    NameSpaceName = nameSpaceName,

                };
                File.WriteAllText(outFileName, ConverterLogic.SvgDirToXaml(CurrentDir, resKeyInfo, false));

                BuildBatchFile(outFileName, resKeyInfo);
            }
        }

        private void BuildBatchFile(string outFileName, ResKeyInfo compResKeyInfo)
        {
            if (MessageBox.Show(outFileName + "\nhas been written\nCreate a BatchFile to automate next time?",
                null, MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            {
                string outputName = Path.GetFileNameWithoutExtension(outFileName);
                string outputDir = Path.GetDirectoryName(outFileName);
                string relOutputDir = FileUtils.MakeRelativePath(CurrentDir, PathIs.Folder, outputDir, PathIs.Folder);
                string svgToXamlPath = System.Reflection.Assembly.GetEntryAssembly( ).Location;
                string relSvgToXamlPath = FileUtils.MakeRelativePath(CurrentDir, PathIs.Folder, svgToXamlPath, PathIs.File);
                string batchText = $"{relSvgToXamlPath} BuildDict /inputdir \".\" /outputdir \"{relOutputDir}\" /outputname {outputName}";

                if (compResKeyInfo.UseComponentResKeys)
                {
                    batchText += $" /useComponentResKeys=true /compResKeyNSName={compResKeyInfo.NameSpaceName} /compResKeyNS={compResKeyInfo.NameSpace}";
                    WriteT4Template(outFileName);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(compResKeyInfo.Prefix))
                    {
                        batchText += " /nameprefix \"" + compResKeyInfo.Prefix + "\"";
                    }
                }

                batchText += "\r\npause";

                File.WriteAllText(Path.Combine(CurrentDir, "Update.cmd"), batchText);
            }
        }

        private static void WriteT4Template(string outFileName)
        {
            //BuildAction: "Embedded Resource"
            Type appType = typeof(App);
            System.Reflection.Assembly assembly = appType.Assembly;
            //assembly.GetName().Name
            string resourceName = appType.Namespace + "." + "Payload.T4Template.tt"; //Achtung: hier Punkt statt Slash
            Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidDataException($"Error: {resourceName} not found in payload file");
            string text = new StreamReader(stream, Encoding.UTF8).ReadToEnd( );
            string t4FileName = Path.ChangeExtension(outFileName, ".tt");
            File.WriteAllText(t4FileName, text, Encoding.UTF8);
        }

        private void InfoExecute( )
        {
            MessageBox.Show("SvgToXaml\n\nPowered by\nsharpvectors.codeplex.com (SVG Support),\nICSharpCode (AvalonEdit)", "Info");
        }
        private void OpenExplorerExecute(string path)
        {
            Process.Start(path);
        }

        public static SvgImagesViewModel DesignInstance
        {
            get
            {
                SvgImagesViewModel result = new SvgImagesViewModel( );
                result.Images.Add(SvgImageViewModel.DesignInstance);
                result.Images.Add(SvgImageViewModel.DesignInstance);
                return result;
            }
        }

        public string CurrentDir
        {
            get => _currentDir;
            set
            {
                if (SetProperty(ref _currentDir, value))
                    ReadImagesFromDir(_currentDir);
            }
        }

        public ImageBaseViewModel SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ObservableCollectionSafe<ImageBaseViewModel> Images
        {
            get => _images;
            set => SetProperty(ref _images, value);
        }

        public ICommand OpenFolderCommand { get; set; }
        public ICommand OpenFileCommand { get; set; }
        public ICommand ExportDirCommand { get; set; }
        public ICommand InfoCommand { get; set; }

        public ObservableCollection<Tuple<object, ICommand>> ContextMenuCommands { get; }

        private void ReadImagesFromDir(string folder)
        {
            Images.Clear( );
            System.Collections.Generic.IEnumerable<string> svgFiles = ConverterLogic.SvgFilesFromFolder(folder);
            System.Collections.Generic.IEnumerable<SvgImageViewModel> svgImages = svgFiles.Select(f => new SvgImageViewModel(f));

            string[] graphicFiles = GetFilesMulti(folder, GraphicImageViewModel.SupportedFormats);
            System.Collections.Generic.IEnumerable<GraphicImageViewModel> graphicImages = graphicFiles.Select(f => new GraphicImageViewModel(f));

            IOrderedEnumerable<ImageBaseViewModel> allImages = svgImages.Concat<ImageBaseViewModel>(graphicImages).OrderBy(e => e.FilePath);

            Images.AddRange(allImages);
        }

        private static string[] GetFilesMulti(string sourceFolder, string filters, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            try
            {
                return !Directory.Exists(sourceFolder)
                    ? Array.Empty<string>( )
                    : filters.Split('|').SelectMany(filter => Directory.GetFiles(sourceFolder, filter, searchOption)).ToArray( );
            }
            catch (Exception)
            {
                return Array.Empty<string>( );
            }
        }

        public void Dispose( )
        {
            _images.Dispose( );
            GC.SuppressFinalize(this);
        }
    }
}
