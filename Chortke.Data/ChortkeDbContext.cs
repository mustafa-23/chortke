using Microsoft.EntityFrameworkCore;

namespace Chortke.Data;

public class ChortkeDbContext(DbContextOptions<ChortkeDbContext> options) : DbContext(options)
{
	public DbSet<Book> Books => Set<Book>();
	public DbSet<Document> Documents => Set<Document>();
	public DbSet<Article> Articles => Set<Article>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Book>(entity =>
		{
			entity.Property(b => b.Name).IsRequired();

			entity.HasOne(b => b.Parent)
				.WithMany(b => b.Children)
				.HasForeignKey(b => b.ParentId)
				.OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<Document>(entity =>
		{
			entity.Property(x => x.Description).HasMaxLength(256);
		});

		modelBuilder.Entity<Article>(entity =>
		{
			entity.Property(x => x.Description).HasMaxLength(128);
			entity.Property(x => x.Tag).HasMaxLength(256);

			entity.HasOne(x => x.Book)
				.WithMany(x => x.Articles)
				.HasForeignKey(x => x.BookId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasOne(x => x.Document)
				.WithMany(x => x.Articles)
				.HasForeignKey(x => x.DocumentId)
				.OnDelete(DeleteBehavior.Restrict);
		});
	}
}