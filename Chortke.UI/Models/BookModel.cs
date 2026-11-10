using Moonlight;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Chortke.Models;

public class BookModel : ModelBase
{
	public string Name { get => GetProperty(() => Name); set => SetProperty(() => Name, value); }
	public long InitialAmount { get => GetProperty(() => InitialAmount); set => SetProperty(() => InitialAmount, value); }
	public bool IsCredit { get => GetProperty(() => IsCredit); set => SetProperty(() => IsCredit, value); }
	public int? ParentId { get; set; }

	[Ignore]
	public bool HasChildren { get => GetProperty(() => HasChildren); set => SetProperty(() => HasChildren, value); }
	[Ignore]
	public bool ChildrenLoaded { get; set; }
	[Ignore]
	public bool IsPlaceHolder { get; set; }
	[Ignore]
	public BookModel? Parent { get; set; }
	[Ignore]
	public ObservableCollection<BookModel> Children { get => GetProperty(() => Children); set => SetProperty(() => Children, value); }
	[Ignore]
	public List<ArticleModel> Articles { get; set; } = default!;

	public string FullPath => (Parent == null ? string.Empty : (Parent.FullPath + "/")) + Name;
	public string Path => FullPath[..^Name.Length];

	public BookModel()
	{
		Children = [];
	}

	public override string ToString() => $"[{Name},{Children?.Count ?? 0}]";
}