using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using TestApplication.Models;
using System.IO;
using System.Threading.Tasks;

namespace TestApplication.Utils
{
    class DBConnectionNotConfiguredException: Exception {
        public DBConnectionNotConfiguredException(string message) : base(message) { }
    }
    public static class DbWorker
    {
        private static AppDbContext? dbContext = null;
        public static bool CheckDbContext()
        {
            if (dbContext == null)
            {
                try
                {
                    dbContext = new AppDbContext();
                    dbContext.Database.EnsureCreated();
                    return true;
                }
                catch
                {
                    dbContext = null;
                    return false;
                }
            }
            return true;
        }
        public static async Task<long> ImportSpecifiedFileToDatabase(string filePath, bool dropInvalidRows, WorkerDoTaskHandler onProcess)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException($"File {filePath} not found");
            if (CheckDbContext() && dbContext != null)
            {
                var fileInfo = new FileInfo(filePath);
                long rowCount = (long)Math.Round(fileInfo.Length / 69m); //69 length in bytes of row
                dbContext.TruncateRandomRowsTable();
                long row = 0;
                long deletedRows = 0;
                using (var file = File.OpenText(filePath))
                {
                    var lastUpdateDate = DateTime.Now;
                    while (!file.EndOfStream)
                    {
                        var line = file.ReadLine();
                        if(line != null)
                        {
                            row++;
                            var values = line.Split("||", StringSplitOptions.RemoveEmptyEntries);
                            try
                            {
                                var randomRow = new RandomRow()
                                {
                                    RandomDate = DateOnly.Parse(values[0]),
                                    LatinString = values[1],
                                    RussianString = values[2],
                                    EvenNumber = int.Parse(values[3]),
                                    FloatNumber = decimal.Parse(values[4],System.Globalization.NumberStyles.AllowDecimalPoint),
                                };
                                dbContext.RandomRows.Add(randomRow);
                                if((DateTime.Now - lastUpdateDate).TotalSeconds > 3)
                                {
                                    lastUpdateDate = DateTime.Now;
                                    await dbContext.SaveChangesAsync();
                                    onProcess(new WorkerEventArgs(row,rowCount));
                                }
                            }
                            catch(Exception ex)
                            {
                                if (dropInvalidRows)
                                {
                                    deletedRows++;
                                    continue;
                                }
                                else
                                {
                                    throw new Exception($"Invalid row in file {row}){line}\n{ex}");
                                }
                            }
                        }
                    }
                }
                await dbContext.SaveChangesAsync();
                return deletedRows;
            }
            throw new DBConnectionNotConfiguredException("Unable to connect to database server");
        }
    }
}
/*
 public void TruncateRandomRowsTable()
        {
            Database.ExecuteSqlRaw("TRUNCATE TABLE \"RandomRows\"");
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var host = ConfigurationManager.AppSettings["postgresql_host"];
                var port = ConfigurationManager.AppSettings["postgresql_port"];
                var db = ConfigurationManager.AppSettings["postgresql_database"];
                var user = ConfigurationManager.AppSettings["postgresql_username"];
                var password = ConfigurationManager.AppSettings["postgresql_password"];
                optionsBuilder.UseNpgsql($"Host={host};Port={port};Database={db};Username={user};Password={password}");
            }
        }
 */