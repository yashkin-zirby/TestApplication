using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Syncfusion.XlsIO.Implementation;
using System.Windows;

namespace TestApplication.Utils
{
    class ExcelAccountingReader : IDisposable
    {
        private static ExcelEngine? engine = null;
        private static IApplication? application = null;
        private IWorkbook workbook;
        public ExcelAccountingReader(string fileName)
        {
            if(engine == null)
            {
                engine = new ExcelEngine();
            }
            if(application == null)
            {
                application = engine.Excel;
                application.DefaultVersion = ExcelVersion.Xlsx;
            }

            FileStream inputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            workbook = application.Workbooks.Open(inputStream);
            workbook.Worksheets[0].ExportDataTableEvent += new WorksheetImpl.ExportDataTableEventHandler((args) =>
            {
                string cell = $"{args.ExcelValue}";
                decimal value;
                if (!decimal.TryParse(cell, out value) || args.ExcelColumnIndex == 1 && value < 1000)
                {
                    args.ExportDataTableAction = ExportDataTableActions.SkipRow;
                    return;
                }
                args.DataTableValue = value;
            });
        }
        public int LastRow { get
            {
                return workbook.Worksheets[0].UsedRange.End.Row;
            } 
        }
        public DataTable ReadDataRowsInRange(int startRow, int length)
        {
            return workbook.Worksheets[0].ExportDataTable(startRow, 1, length, 5,
                ExcelExportDataTableOptions.None);
        }
        public string ReadString(string cell)
        {
            return workbook.Worksheets[0].Range[cell].Value;
        }
        public double? ReadNumber(string cell)
        {
            return workbook.Worksheets[0].Range[cell].HasNumber? workbook.Worksheets[0].Range[cell].Number : null;
        }

        public void Dispose()
        {
            workbook.Close();
        }
    }
}
