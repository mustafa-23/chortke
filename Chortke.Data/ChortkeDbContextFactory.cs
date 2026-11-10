using Microsoft.EntityFrameworkCore;

namespace Chortke.Data
{
	public class ChortkeDbContextFactory(string dbPath)
	{
		private string _dbPath = dbPath;
		public ChortkeDbContext CreateDbContext()
		{
			var options = new DbContextOptionsBuilder<ChortkeDbContext>()
				.UseSqlite($"Data Source={_dbPath}")
				.Options;

			return new ChortkeDbContext(options);
		}
		public void ChangeDatabase(string dbPath) => _dbPath = dbPath;
	}
}
