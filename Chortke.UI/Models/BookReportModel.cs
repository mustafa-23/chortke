using System.Collections.Generic;
using System.Linq;

namespace Chortke.Models
{
	public class BookReportModel
	{
		public int Id { get; set; }
		public int? ParentId { get; set; }
		public string Name { get; set; } = default!;
		public int InitialAmount { get; set; }
		private int _totalCredit;
		public int TotalCredit
		{
			get => Children?.Count > 0 ? Children.Sum(b => b.TotalCredit) : _totalCredit;
			set => _totalCredit = value;
		}
		private int _totalDebit;
		public int TotalDebit { get => Children?.Count > 0 ? Children.Sum(b => b.TotalDebit) : _totalDebit; set => _totalDebit = value; }
		public int Remain => InitialAmount + (TotalCredit - TotalDebit);
		public List<ArticleModel> Articles { get; set; } = default!;
		public List<BookReportModel> Children { get; set; } = default!;
	}
}
