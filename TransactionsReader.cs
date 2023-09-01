using System.Globalization;

namespace TransactionsImport
{
	internal class TransactionsReader
	{
		public string FilePath { get; }

		public TransactionsReader(string csvFilePath)
		{
			FilePath = csvFilePath;
		}

		public IEnumerable<AccountTransaction> Read()
		{
			return File.ReadLines(FilePath).Select(ParseLine);
		}

		private AccountTransaction ParseLine(string csvLine)
		{
			var columns = csvLine.Trim('\"').Split("\",\"");
			if (columns.Length != 6)
			{
				var ex = new ArgumentException("The CSV line invalid format. It is expected to contain 6 columns.", nameof(csvLine));
				ex.Data.Add("Expected format", "\"Value date\",\"Processing date\",\"Debit\",\"Credit\",\"Amount\",\"Description\"");
				ex.Data.Add(nameof(csvLine), csvLine);
				throw ex;
			}

			var tran = new AccountTransaction();
			tran.Date = DateTime.ParseExact(columns[1], "dd.MM.yyyy", CultureInfo.InvariantCulture);
			tran.Debit = !string.IsNullOrEmpty(columns[2]) ? Convert.ToDecimal(columns[2], CultureInfo.InvariantCulture) : 0m;
			tran.Credit = !string.IsNullOrEmpty(columns[3]) ? Convert.ToDecimal(columns[3], CultureInfo.InvariantCulture) : 0m;
			tran.CurrentAmount = !string.IsNullOrEmpty(columns[4]) ? Convert.ToDecimal(columns[4], CultureInfo.InvariantCulture) : null;
			tran.Description = columns[5].Trim();
			return tran;
		}
	}
}
