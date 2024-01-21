using Microsoft.Win32;
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
using System.Windows.Shapes;
using TestApplication.Utils;

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

        private void ExportExcelDataButton_Click(object sender, RoutedEventArgs e)
        {
            var result = openFileDialog.ShowDialog();
            if(result.HasValue && result.Value)
            {
                try
                {
                    ExcelAccountingReader reader = new ExcelAccountingReader(openFileDialog.FileName);
                    var dt = reader.ReadDataRowsInRange(5, 12);
                    var rows = dt.Rows;
                    MessageBox.Show(dt.Rows.Count.ToString());
                    for(int i = 0; i < rows.Count; i++)
                    {
                        MessageBox.Show($"{rows[i][0]}|{rows[i][1]}|{rows[i][2]}|{rows[i][3]}|{rows[i][4]}");
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
