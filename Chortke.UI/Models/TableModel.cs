using DevExpress.Mvvm;
using System.Collections.Generic;

namespace Chortke.Models
{
	public class TableModel(string title, string displayName) : BindableBase
	{
		public string Title { get; } = title;
		public string DisplayName { get; } = displayName;
		public string Dependencies { get; set; } = default!;
		public bool Keep { get => GetProperty(() => Keep); set => SetProperty(() => Keep, value); }
		public int RowCount { get => GetProperty(() => RowCount); set => SetProperty(() => RowCount, value); }

		public static List<TableModel> GetTables() =>
		[
			new TableModel("Book","دفاتر"),
			new TableModel("Document", "اسناد"){Dependencies = "Books,Articles"},
			new TableModel("Article", "موارد سند"){Dependencies = "Books"},
		];
	}
}
