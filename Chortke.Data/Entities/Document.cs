namespace Chortke.Data;

public class Document
{
	public int Id { get; set; }
	public DateTime CreateDate { get; set; }
	public string? Description { get; set; }

	public ICollection<Article> Articles { get; set; } = [];
}