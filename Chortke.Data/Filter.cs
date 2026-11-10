namespace Chortke.Data
{
	public class Filter
	{
		public string Description { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public List<int>? BookIds { get; set; }
	}
}
