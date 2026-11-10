using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Chortke.Data;

public static class DBHandler
{
	private static string _dbName = "chortke.db";
#if DEBUG
	private static string DBPath => _dbName;
#else
	private static string DBPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Chortke", _dbName);
#endif

	static DBHandler()
	{
		Initialize();
	}
	private static readonly ChortkeDbContextFactory _factory = new(DBPath);
	public static void Initialize()
	{
		EnsureDatabaseDirectoryExists();
		using var context = _factory.CreateDbContext();
		context.Database.Migrate();
	}

	private static TResult ExecuteWithContext<TResult>(Func<ChortkeDbContext, TResult> operation)
	{
		ArgumentNullException.ThrowIfNull(operation);
		using var context = _factory.CreateDbContext();
		return operation(context);
	}
	private static async Task<TResult> ExecuteWithContextAsync<TResult>(Func<ChortkeDbContext, Task<TResult>> operation)
	{
		ArgumentNullException.ThrowIfNull(operation);
		await using var context = _factory.CreateDbContext();
		return await operation(context);
	}

	[Obsolete]
	public static int Delete<T>(T entity) where T : class
	{
		return ExecuteWithContext(ctx =>
		{
			if (entity != null)
				ctx.Remove(entity);
			return ctx.SaveChanges();
		});
	}

	#region Book
	public static int CreateBook(Book book)
	{
		return ExecuteWithContext(ctx =>
		{
			ctx.Books.Add(book);
			ctx.SaveChanges();
			return book.Id;
		});
	}
	public static Book? GetBook(int id) => ExecuteWithContext(ctx => ctx.Books.Find(id));
	public static int UpdateBookChanges(IEnumerable<Book> books)
	{
		ArgumentNullException.ThrowIfNull(books);

		return ExecuteWithContext(ctx =>
		{
			foreach (var book in books)
			{
				ArgumentNullException.ThrowIfNull(book);
				if (book.Id == 0)
				{
					ctx.Books.Add(book);
					continue;
				}

				var existingBook = ctx.Books.Find(book.Id);
				if (existingBook is null)
					continue;
				existingBook.Name = book.Name;
				existingBook.InitialAmount = book.InitialAmount;
				existingBook.IsCredit = book.IsCredit;
			}
			return ctx.SaveChanges();
		});
	}
	public static bool DeleteBook(int id)
	{
		return ExecuteWithContext(ctx =>
		{
			var book = ctx.Books.Find(id);
			if (book is null)
				return false;
			ctx.Books.Remove(book);
			ctx.SaveChanges();
			return true;
		});
	}

	public static Task<List<int>> GetBookParentIdsAsync()
	{
		return ExecuteWithContextAsync(ctx => ctx.Books.Where(x => x.ParentId != null).Select(x => x.ParentId!.Value).Distinct().ToListAsync());
	}
	public static List<Book> GetRootBooks() => ExecuteWithContext(ctx => ctx.Books.Where(b => b.ParentId == null).ToList());
	public static Task<List<Book>> GetRootBooksAsync() => ExecuteWithContextAsync(ctx => ctx.Books.Where(book => book.ParentId == null).ToListAsync());
	public static async Task<List<Book>> GetBookChildrenAsync(int parentId) => await ExecuteWithContextAsync(ctx => ctx.Books.Where(x => x.ParentId == parentId).ToListAsync());
	public static List<Book> GetLeaves() => ExecuteWithContext(ctx => ctx.Books.Where(book => !ctx.Books.Any(child => child.ParentId == book.Id)).ToList());
	#endregion

	#region Document
	public static int SaveDocument(Document document)
	{
		ArgumentNullException.ThrowIfNull(document);
		if (document.Articles is null || document.Articles.Count == 0)
			throw new InvalidOperationException("A document must contain at least one article.");

		// Validate accounting balance
		long debit = 0, credit = 0;

		foreach (var article in document.Articles)
		{
			if (article.IsDebit)
				debit = checked(debit + article.Amount);
			else
				credit = checked(credit + article.Amount);
		}

		if (debit != credit)
			throw new InvalidOperationException("Document debit and credit totals must be equal.");

		return ExecuteWithContext(ctx =>
		{
			if (document.Id == 0)
				ctx.Documents.Add(document);
			else
			{
				var existing = ctx.Documents
					.Include(d => d.Articles)
					.FirstOrDefault(d => d.Id == document.Id)
					?? throw new InvalidOperationException("Document not found.");

				existing.CreateDate = document.CreateDate;
				existing.Description = document.Description;

				// Remove deleted articles.
				var submittedIds = document.Articles
					.Where(a => a.Id != 0)
					.Select(a => a.Id)
					.ToHashSet();

				var deletedArticles = existing.Articles
					.Where(a => !submittedIds.Contains(a.Id))
					.ToList();

				ctx.Articles.RemoveRange(deletedArticles);

				// Update existing articles and add new ones
				foreach (var article in document.Articles)
				{
					if (article.Id == 0)
					{
						existing.Articles.Add(article);
						continue;
					}

					var existingArticle = existing.Articles.FirstOrDefault(a => a.Id == article.Id)
						?? throw new InvalidOperationException($"Article {article.Id} does not belong to this document.");

					existingArticle.Amount = article.Amount;
					existingArticle.IsDebit = article.IsDebit;
					existingArticle.CreateDate = article.CreateDate;
					existingArticle.BookId = article.BookId;
					existingArticle.Description = article.Description;
					existingArticle.Tag = article.Tag;
				}
			}
			return ctx.SaveChanges();
		});

	}
	public static IEnumerable<Document> GetDocuments() => ExecuteWithContext(ctx => ctx.Documents.Include(x => x.Articles).ToList());
	#endregion

	#region Articles
	public static List<Article> GetArticlesByBookId(int bookId) => ExecuteWithContext(ctx => ctx.Articles.Where(x => x.BookId == bookId).ToList());
	public static List<Article> GetArticles(Filter filter)
	{
		ArgumentNullException.ThrowIfNull(filter);
		return ExecuteWithContext(ctx =>
		{
			IQueryable<Article> query = ctx.Articles;
			if (filter.BookIds is { Count: > 0 })
				query = query.Where(a => filter.BookIds.Contains(a.BookId));

			if (filter.FromDate.HasValue)
				query = query.Where(a => a.CreateDate >= filter.FromDate.Value);

			if (filter.ToDate.HasValue)
				query = query.Where(a => a.CreateDate <= filter.ToDate.Value);

			if (!string.IsNullOrWhiteSpace(filter.Description))
			{
				query = query.Where(a =>
					a.Description != null &&
					a.Description.Contains(filter.Description));
			}

			return query.ToList();
		});
	}
	#endregion

	/// <summary>
	/// Calculates the credit and debit totals for articles belonging
	/// directly to the specified book.
	/// </summary>
	public static (decimal Credit, decimal Debit) GetSumDC(int bookId)
	{
		return ExecuteWithContext(ctx =>
		{
			var totals = ctx.Articles
				.Where(a => a.BookId == bookId)
				.GroupBy(a => a.IsDebit)
				.Select(g => new
				{
					IsDebit = g.Key,
					Amount = g.Sum(a => a.Amount)
				})
				.ToList();

			decimal credit = totals.Where(x => !x.IsDebit)
				.Select(x => x.Amount)
				.FirstOrDefault();
			decimal debit = totals.Where(x => x.IsDebit)
				.Select(x => x.Amount)
				.FirstOrDefault();

			return (credit, debit);
		});
	}

	public static int ClearTable(ChortkeTable table)
	{
		return ExecuteWithContext(ctx =>
		{
			return table switch
			{
				ChortkeTable.Book => ctx.Books.ExecuteDelete(),
				ChortkeTable.Article => ctx.Articles.ExecuteDelete(),
				ChortkeTable.Document => ctx.Documents.ExecuteDelete(),
				_ => throw new ArgumentOutOfRangeException(nameof(table)),
			};
		});
	}
	public static int GetRowCount(ChortkeTable table)
	{
		return ExecuteWithContext(ctx => table switch
			{
				ChortkeTable.Book => ctx.Set<Book>().Count(),
				ChortkeTable.Article => ctx.Set<Article>().Count(),
				ChortkeTable.Document => ctx.Set<Document>().Count(),
				_ => throw new ArgumentOutOfRangeException(nameof(table)),
			});
	}
	public static void ChangeDb(string dbName)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(dbName);

		_dbName = dbName;
		_factory.ChangeDatabase(DBPath);

		Initialize();
	}
	private static void EnsureDatabaseDirectoryExists()
	{
		var directory = Path.GetDirectoryName(DBPath);

		if (!string.IsNullOrEmpty(directory))
			Directory.CreateDirectory(directory);
	}
}