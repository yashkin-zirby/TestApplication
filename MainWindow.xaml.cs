using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using TestApplication.Utils;
using TestApplication.Windows;
using static System.Net.WebRequestMethods;

namespace TestApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CommonOpenFileDialog openDirDialog = new CommonOpenFileDialog() {IsFolderPicker=true };
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        OpenFileDialog openFileDialog = new OpenFileDialog();
        string? workDir = null;
        public MainWindow()
        {
            InitializeComponent();
            saveFileDialog.Filter = "Text File|*.txt";
            saveFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            saveFileDialog.Title = "Choose file for save result of combine";
            saveFileDialog.DefaultExt = ".txt";
            saveFileDialog.AddExtension = true;

            openFileDialog.Filter = "Text File|*.txt";
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog.Title = "Choose file for import to database";
            openFileDialog.DefaultExt = ".txt";

            CountOfFiles.ItemsSource = new int[] {1,2,5,10,20 };
            openDirDialog.InitialDirectory = Directory.GetCurrentDirectory();
            openDirDialog.Title = "Choose directory for save generated files";
        }
        private void GenerateFilesButton_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(workDir))
            {
                MessageBox.Show("First select your work directory");
                return;
            }
            int fileCount = (int)CountOfFiles.SelectedValue;
            ProgressWindow.Visibility = Visibility.Visible;
            TaskProgressBar.Value = 0;
            TaskProgressBar.Maximum = fileCount;
            ProgressBarTitle.Text = "Files generation";
            Task.Run(() => Utils.FileWorker.GenerateSpecialFiles(workDir, fileCount, args => {
                Dispatcher.Invoke(() => {
                    TaskProgressBar.Value = args.Progress;
                    if (args.Progress == args.Count) ProgressWindow.Visibility = Visibility.Collapsed;
                });
            }));
        }
        private void CombineFilesButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(workDir))
            {
                MessageBox.Show("First select your work directory");
                return;
            }
            var files = Directory.GetFiles(workDir);
            if(files.Length == 0)
            {
                MessageBox.Show("Current work directory is empty.\n Please generate files");
                return;
            }
            var result = saveFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var resultFile = saveFileDialog.FileName;
                ProgressWindow.Visibility = Visibility.Visible;
                TaskProgressBar.Value = 0;
                TaskProgressBar.Maximum = files.Length;
                ProgressBarTitle.Text = "Processing files";
                ProgressBarDescription.Text = "";
                string filter = "";
                if(UseFilter.IsChecked.HasValue && UseFilter.IsChecked.Value)
                {
                    filter = RowFilter.Text;
                }
                Utils.FileWorker.CombineSpecialFiles(workDir, resultFile , filter, args =>
                {
                    var combineArgs = args as WorkerCombineEventArgs;
                    Dispatcher.Invoke(new Action(() => {
                        TaskProgressBar.Value = args.Progress;
                        if (combineArgs != null)
                        {
                            ProgressBarDescription.Text = $"Rows deleted: {combineArgs.DeletedRows}";
                        }
                    }));
                }).ContinueWith(result=> {
                    Dispatcher.Invoke(() => {
                        ProgressWindow.Visibility = Visibility.Collapsed;
                        MessageBox.Show($"Rows removed during combine process: {result.Result}");
                    });
                });
            }
        }
        private void WorkDirectory_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var result = openDirDialog.ShowDialog();
            if (CommonFileDialogResult.Ok == result)
            {
                workDir = openDirDialog.FileName;
                openFileDialog.InitialDirectory = workDir;
                saveFileDialog.InitialDirectory = workDir;
                WorkDirectory.Text = workDir;
            }
        }

        private void ImportToDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            var result = openFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var resultFile = openFileDialog.FileName;
                bool dropRows = DropInvalidRows.IsChecked.HasValue && DropInvalidRows.IsChecked.Value;
                ProgressWindow.Visibility = Visibility.Visible;
                TaskProgressBar.Value = 0;
                TaskProgressBar.Maximum = 1;
                ProgressBarTitle.Text = "Import rows to database";
                ProgressBarDescription.Text = "";
                var task = Task.Run(async ()=> await DbWorker.ImportSpecifiedFileToDatabase(resultFile, dropRows, args =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        TaskProgressBar.Value = args.Progress;
                        TaskProgressBar.Maximum = args.Count;
                        ProgressBarDescription.Text = $"Rows processed: {args.Progress} / {args.Count}";
                    });
                }));
                task.ContinueWith(t =>
                {
                    var exception = t.Exception;
                    if (exception != null)
                    {
                        foreach (var ex in exception.InnerExceptions) {
                            MessageBox.Show(ex.Message);
                        }
                        Dispatcher.Invoke(() =>
                        {
                            ProgressWindow.Visibility = Visibility.Collapsed;
                        });
                        if (exception.InnerExceptions.Any(ex => ex is DBConnectionNotConfiguredException))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                new DatabaseConfigurationWindow().ShowDialog();
                            });
                        }
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
                task.ContinueWith(result =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProgressWindow.Visibility = Visibility.Collapsed;
                        if (!dropRows) MessageBox.Show($"Rows droped in import process: {result.Result}");
                    });
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

            }
        }
        
    }
}
