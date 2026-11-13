namespace Chortke.Data;

public class Article
{
	public int Id { get; set; }
	public long Amount { get; set; }
	public bool IsDebit { get; set; }
	public DateTime CreateDate { get; set; }
	public int BookId { get; set; }
	public int DocumentId { get; set; }
	public string? Description { get; set; }
	public string? Tag { get; set; } = string.Empty;

	public Book Book { get; set; } = default!;
	public Document Document { get; set; } = default!;
}