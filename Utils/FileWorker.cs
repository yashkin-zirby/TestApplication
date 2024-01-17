using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestApplication.Utils
{
    public class FileWorker
    {
        private static Random random = new Random();
        private static string GenerateRow()
        {
            var start = DateTime.Today.AddYears(-5);
            var range = (DateTime.Today - start).Days;
            DateTime date = start.AddDays(random.Next(range));
            StringBuilder latinString = new StringBuilder(10);
            for(int i = 0; i < 10; i++)
            {
                char symbol = (char)random.Next('a', 'z' + 1);
                latinString.Append(random.Next() % 2 == 0 ? char.ToUpper(symbol) : symbol);
            }
            StringBuilder russianString = new StringBuilder(10);
            for (int i = 0; i < 10; i++)
            {
                char symbol = (char)random.Next('а', 'я' + 1);
                russianString.Append(random.Next() % 2 == 0 ? char.ToUpper(symbol) : symbol);
            }
            int evenNumber = random.Next(2, 100000000);
            evenNumber = evenNumber - (evenNumber & 1);
            double floatingNumber = random.NextDouble()+random.Next(1,20);
            return $"{date.ToShortDateString()}||{latinString}||{russianString}||{evenNumber:00000000}||{floatingNumber.ToString("0.00000000")}||";
        }
        private static void GenerateFile(string path)
        {
            using (var file = new StreamWriter(File.Create(path)))
            {
                for (int i = 0; i < 100000; i++)
                {
                    file.WriteLine(GenerateRow());
                }
            }
        }
        public static void GenerateSpecialFiles(string directoryName, int fileCount, WorkerDoTaskHandler onGenerated)
        {
            string path = directoryName;
            Directory.CreateDirectory(path);
            int progress = 0;
            Parallel.For(0, fileCount, i => {
                string filePath = $"{path}/file{i}_{DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss")}.txt";
                GenerateFile(filePath);
                Interlocked.Increment(ref progress);
                onGenerated(new WorkerEventArgs(progress, fileCount));
            });
        }
        public static async Task<int> CombineSpecialFiles(string directory, string resultFilePath, string removeRowPattern, WorkerDoTaskHandler onProccesed)
        {
            if(!Directory.Exists(directory))throw new DirectoryNotFoundException($"Дирректория {directory} не найдена");
            var files = Directory.GetFiles(directory);
            if(files.Length == 0)throw new FileNotFoundException($"Не найдены файлы для объединения в дирректории {directory}");
            var deletedRows = 0;
            if(File.Exists(resultFilePath))File.Delete(resultFilePath);
            for (int i = 0; i < files.Length; i++)
            {
                var rows = await File.ReadAllLinesAsync(files[i]);
                int rowsCount = rows.Length;
                if (!string.IsNullOrEmpty(removeRowPattern)) rows = rows.Where(n => !n.Contains(removeRowPattern)).ToArray();
                deletedRows += rowsCount - rows.Length;
                File.AppendAllLines(resultFilePath, rows);
                File.Delete(files[i]);
                onProccesed(new WorkerCombineEventArgs(i+1, files.Length, rowsCount - rows.Length));
            }
            return deletedRows;
        }
    }
}
