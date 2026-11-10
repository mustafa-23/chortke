using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace Chortke.Data;

public class DesignTimeChortkeDbContextFactory : IDesignTimeDbContextFactory<ChortkeDbContext>
{
	public ChortkeDbContext CreateDbContext(string[] args)
	{
		var options = new DbContextOptionsBuilder<ChortkeDbContext>()
			.UseSqlite("Data Source=chortke.db")
			.Options;
		return new ChortkeDbContext(options);
	}
}
