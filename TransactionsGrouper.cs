using System.Text;

namespace TransactionsImport
{
	internal class TransactionsGrouper
	{
		public IEnumerable<AccountTransaction> Group(IEnumerable<AccountTransaction> transactions)
		{
			List<AccountTransaction> transactionsWithCategories = new List<AccountTransaction>();
			foreach (AccountTransaction tran in transactions)
			{
				if (tran.Category == null)
					yield return tran;
				else
					transactionsWithCategories.Add(tran);
			}

			var grouppedTransactions = transactionsWithCategories.GroupBy(
				t => (t.Date, t.Category, t.Account, t.Budget, t.FriendlyDescription),
				CreateGroupTransaction);

			foreach (AccountTransaction tran in grouppedTransactions)
			{
				yield return tran;
			}
		}

		private AccountTransaction CreateGroupTransaction(
			(DateTime Date, string? Category, string? Account, string? Budget, string? FriendlyDescription) key,
			IEnumerable<AccountTransaction> transactionsGroup)
		{
			if (transactionsGroup.Count() == 1)
			{
				return transactionsGroup.First();
			}

			decimal amount = 0m;
			StringBuilder? descriptionBuilder = null;
			foreach (var tran in transactionsGroup)
			{
				amount += tran.Debit - tran.Credit;

				if (tran.Description != null)
				{
					if (descriptionBuilder == null)
					{
						descriptionBuilder = new StringBuilder(tran.Description);
					}
					else
					{
						descriptionBuilder.Append("; ");
						descriptionBuilder.Append(tran.Description);
					}
				}
			}

			return new AccountTransaction
			{
				Date = key.Date,
				Debit = amount > 0m ? amount : 0m,
				Credit = amount < 0m ? -amount : 0m,
				CurrentAmount = null,
				Description = descriptionBuilder != null ? descriptionBuilder.ToString() : null,

				Account = key.Account,
				Budget = key.Budget,
				Category = key.Category,
				FriendlyDescription = key.FriendlyDescription
			};
		}
	}
}
