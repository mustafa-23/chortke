namespace Chortke.Data;

public class Book
{
	public int Id { get; set; }
	public int? ParentId { get; set; }
	public string Name { get; set; } = string.Empty;
	public long InitialAmount { get; set; }
	public bool IsCredit { get; set; }
	public Book? Parent { get; set; }

	public ICollection<Book> Children { get; set; } = [];
	public ICollection<Article> Articles { get; set; } = [];
}
