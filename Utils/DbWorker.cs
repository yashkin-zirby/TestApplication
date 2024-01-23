using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using TestApplication.Models;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace TestApplication.Utils
{
    class DBConnectionNotConfiguredException: Exception {
        public DBConnectionNotConfiguredException(string message) : base(message) { }
    }
    public static class DbWorker
    {
        private static AppDbContext? dbContext = null;
        public static async Task<bool> CheckDbContext()
        {
            if (dbContext == null)
            {
                try
                {
                    dbContext = new AppDbContext();
                    if (dbContext.Database.EnsureCreated()) {
                        await dbContext.Database.ExecuteSqlRawAsync(@"
                            CREATE VIEW public.""AccountingView"" as
                            select ts.""TurnoverSheet"", ts.""AccountCode"", ts.""Debit"", ts.""Credit"",
                        		(case when ast.""AccountType"" = 'ACTIVE' then ast.""OpeningBalance"" else 0 END) ""OpeningBalanceActive"",
		                        (case when ast.""AccountType"" = 'PASSIVE' then ast.""OpeningBalance"" else 0 END) ""OpeningBalancePassive""
		                        from public.""TurnoverStatement"" ts LEFT JOIN public.""AccountStatement"" ast
			                        on ts.""StatementId"" = ast.""Statement"";
                        ");
                    }
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
        public static async Task<List<TurnoverSheet>> GetTurnoverSheets()
        {
            if (await CheckDbContext() && dbContext != null)
            {
                return dbContext.TurnoverSheets.ToList();
            }
            throw new DBConnectionNotConfiguredException("Unable to connect to database server");
        }
        public static async Task<List<AccountingView>> GetAccountingSheets(int turnoverSheet)
        {
            if (await CheckDbContext() && dbContext != null)
            {
                return dbContext.AccountingView.Where(n => n.TurnoverSheetId == turnoverSheet).ToList();
            }
            throw new DBConnectionNotConfiguredException("Unable to connect to database server");
        }
        public static async Task<long> ImportSpecifiedFileToDatabase(string filePath, bool dropInvalidRows, WorkerDoTaskHandler onProcess)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException($"File {filePath} not found");
            if (await CheckDbContext() && dbContext != null)
            {
                var fileInfo = new FileInfo(filePath);
                long rowCount = (long)Math.Round(fileInfo.Length / 70.5m); //avg length in bytes of row
                dbContext.TruncateRandomRowsTable();
                long row = 0;
                long deletedRows = 0;
                using (var file = File.OpenText(filePath))
                {
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
                                if((row & 65535) == 0)
                                {
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
        public static async void ImportAccountingDataFromExcel(ExcelAccountingReader reader, WorkerDoTaskHandler onProcess)
        {
            if (await CheckDbContext() && dbContext != null)
            {
                string bank = reader.ReadString("A1");
                var date = reader.ReadDateTime("A6");
                string currency = reader.ReadString("G6");
                string filename = System.IO.Path.GetFileName(reader.FileName);
                var sheet = new TurnoverSheet();
                int count = reader.LastRow;
                if (date == null) throw new Exception("Invalid format of data in document");
                sheet.Currency = currency;
                sheet.BankName = bank;
                sheet.FileName = filename;
                sheet.ReportYear = DateOnly.FromDateTime(date.Value);
                dbContext.TurnoverSheets.Add(sheet);
                await dbContext.SaveChangesAsync();
                onProcess(new WorkerEventArgs(1, count));
                var dt = reader.ReadDataRowsInRange(10, count);
                var references = new List<(AccountStatement, TurnoverStatement)>();
                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var code = decimal.Parse($"{dt.Rows[i][0]}");
                        var active = decimal.Parse($"{dt.Rows[i][1]}");
                        var passive = decimal.Parse($"{dt.Rows[i][2]}");
                        var debit = decimal.Parse($"{dt.Rows[i][3]}");
                        var credit = decimal.Parse($"{dt.Rows[i][4]}");
                        var statement = new TurnoverStatement();
                        statement.TurnoverSheet = sheet.SheetId;
                        statement.AccountCode = code;
                        statement.Debit = debit;
                        statement.Credit = credit;
                        dbContext.TurnoverStatements.Add(statement);
                        if (active != 0 || passive != 0)
                        {
                            var accountStatement = new AccountStatement();
                            if (active == 0)
                            {
                                accountStatement.AccountType = "PASSIVE";
                                accountStatement.OpeningBalance = passive;
                            }
                            else
                            {
                                accountStatement.AccountType = "ACTIVE";
                                accountStatement.OpeningBalance = active;
                            }
                            references.Add((accountStatement, statement));
                        }
                        if (references.Count > 20)
                        {
                            await dbContext.SaveChangesAsync();
                            foreach (var reference in references)
                            {
                                var item = reference.Item1;
                                item.Statement = reference.Item2.StatementId;
                                dbContext.AccountStatements.Add(item);
                            }
                            references.Clear();
                            await dbContext.SaveChangesAsync();
                            onProcess(new WorkerEventArgs(10 + i, count));
                        }
                    }
                    await dbContext.SaveChangesAsync();
                    if (references.Count > 0)
                    {
                        foreach (var reference in references)
                        {
                            var item = reference.Item1;
                            item.Statement = reference.Item2.StatementId;
                            dbContext.AccountStatements.Add(item);
                        }
                        references.Clear();
                        await dbContext.SaveChangesAsync();
                    }
                    onProcess(new WorkerEventArgs(count, count));
                }
                catch(Exception e)
                {
                    dbContext.TurnoverSheets.Remove(sheet);
                    dbContext.SaveChanges();
                    throw new Exception($"Error occured while importing data\n{e.Message}");
                }
                return;
            }
            throw new DBConnectionNotConfiguredException("Unable to connect to database server");
        }
    }
}