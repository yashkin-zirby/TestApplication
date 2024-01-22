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
            UpdateExportedFileList();
        }
        private void UpdateExportedFileList()
        {
            try
            {
                ImportedFilesView.ItemsSource = DbWorker.GetTurnoverSheets()
                    .Select(n =>
                    {
                        var button = new Button();
                        button.Content = $"{n.ReportYear}) {n.FileName}";
                        button.Click += (sender, e) =>
                        {
                            var view = DbWorker.GetAccountingSheets(n.SheetId);
                            MessageBox.Show(view.Count.ToString());
                            AccountingTable.ItemsSource = view;
                        };
                        return button;
                    });
            }
            catch (DBConnectionNotConfiguredException e)
            {
                MessageBox.Show(e.Message);
                new DatabaseConfigurationWindow().ShowDialog();
            }catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
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
                    TaskProgressBar.Maximum = reader.LastRow;
                    DbWorker.ImportAccountingDataFromExcel(reader, (args) =>
                    {
                        Dispatcher.Invoke(() => {
                            TaskProgressBar.Value = args.Progress;
                            if(args.Progress == args.Count) {
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
    }
}
;