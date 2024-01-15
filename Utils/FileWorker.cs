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
    public class SpecialFileGeneratedArgs : EventArgs
    {
        private string filePath;
        private int progress;
        private int count;
        public int Count { get { return count; } }
        public int Progress { get { return progress; } }
        public string FilePath { get { return filePath; } }
        public SpecialFileGeneratedArgs(string filePath,int progress, int count) : base() {
            this.filePath = filePath;
            this.progress = progress;
            this.count = count;
        }
    }
    public delegate void SpecialFileGeneratedHandler(SpecialFileGeneratedArgs args);
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
                latinString[i] = random.Next() % 2 == 0 ? char.ToUpper(symbol) : symbol;
            }
            StringBuilder russianString = new StringBuilder(10);
            for (int i = 0; i < 10; i++)
            {
                char symbol = (char)random.Next('а', 'я' + 1);
                russianString[i] = random.Next() % 2 == 0 ? char.ToUpper(symbol) : symbol;
            }
            int evenNumber = random.Next(2, 100000000);
            evenNumber = evenNumber - (evenNumber & 1);
            double floatingNumber = Math.Round(random.NextDouble()+random.Next(1,20),8);
            return $"{date.ToShortDateString()}||{latinString}||{russianString}||{evenNumber}||{}||";
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
        public static void GenerateSpecialFiles(string directoryName, int fileCount, SpecialFileGeneratedHandler onGenerated)
        {
            string path = Path.GetFullPath(directoryName);
            Directory.CreateDirectory(path);
            int progress = 0;
            Parallel.For(0, fileCount, i => {
                string filePath = $"{path}/file{i}_{DateTime.Now}.txt";
                GenerateFile(filePath);
                Interlocked.Increment(ref progress);
                onGenerated(new SpecialFileGeneratedArgs(filePath,progress,fileCount));
            });
        }
    }
}
