using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TestApplication.Models;
using TestApplication.Utils;
using TestApplication.Windows;

namespace TestApplication.Pages
{
    /// <summary>
    /// Логика взаимодействия для WorkWithExcelPage.xaml
    /// </summary>
    public partial class WorkWithExcelPage : Page
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        public WorkWithExcelPage()
        {
            InitializeComponent();
            openFileDialog.Filter = "Excel File|*.xlsx;*.xls";
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog.Title = "Choose file for import to database";
            openFileDialog.DefaultExt = ".xlsx";
        }
        private void UpdateExportedFileList()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            LoadingText.Text = "Loading";
            var task = Task.Run(async()=>await DbWorker.GetTurnoverSheets());
            task.ContinueWith(t =>
            {
                var exception = t.Exception;
                if (exception != null)
                {
                    foreach (var ex in exception.InnerExceptions)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    Dispatcher.Invoke(() =>
                    {
                        Mouse.OverrideCursor = Cursors.Arrow;
                        LoadingText.Text = "";
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
                var buttons = result.Result.Select(n =>
                {
                    var button = new Button();
                    button.Content = $"{n.ReportYear}) {n.FileName}";
                    button.Click += (sender, e) =>
                    {
                        DbWorker.GetAccountingSheets(n.SheetId).ContinueWith(result => {
                            Dispatcher.Invoke(() => {
                                AccountingTable.ItemsSource = result.Result;
                            });
                        }, TaskContinuationOptions.OnlyOnRanToCompletion);
                    };
                    return button;
                });
                Dispatcher.Invoke(() =>
                {
                    Mouse.OverrideCursor = Cursors.Arrow;
                    LoadingText.Text = "";
                    ProgressWindow.Visibility = Visibility.Collapsed;
                    ImportedFilesView.ItemsSource = buttons;
                });
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
        private void ExportExcelDataButton_Click(object sender, RoutedEventArgs e)
        {
            var result = openFileDialog.ShowDialog();
            if(result.HasValue && result.Value)
            {
                try
                {
                    ProgressWindow.Visibility = Visibility.Visible;
                    TaskProgressBar.Value = 0;
                    TaskProgressBar.Maximum = 1;
                    ExcelAccountingReader reader = new ExcelAccountingReader(openFileDialog.FileName);
                    DbWorker.ImportAccountingDataFromExcel(reader, (args) =>
                    {
                        Dispatcher.Invoke(() => {
                            TaskProgressBar.Value = args.Progress;
                            TaskProgressBar.Maximum = args.Count;
                            if (args.Progress == args.Count) {
                                ProgressWindow.Visibility = Visibility.Collapsed;
                                UpdateExportedFileList();
                            }
                        });
                    });
                }
                catch (DBConnectionNotConfiguredException ex)
                {
                    ProgressWindow.Visibility = Visibility.Collapsed;
                    MessageBox.Show(ex.Message);
                    new DatabaseConfigurationWindow().ShowDialog();
                }
                catch (Exception ex)
                {
                    ProgressWindow.Visibility = Visibility.Collapsed;
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateExportedFileList();
        }
    }
}
;