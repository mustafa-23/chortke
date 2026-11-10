using Chortke.Data;
using Chortke.Models;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using Moonlight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Chortke.Common;

public class BookRepository : BindableBase
{
	#region Fields
	private static BookRepository _instance = default!;
	private readonly List<BookModel> _flatBooks = [];
	private readonly Dictionary<BookModel, ObservableCollection<BookModel>> _subscribedChildren = [];
	private readonly Dictionary<int, Task> _loadingTasks = [];
	private HashSet<int> _parentIds = [];
	#endregion

	#region Props
	public static BookRepository Instance => _instance ??= new BookRepository();
	public bool IsLoading { get => GetProperty(() => IsLoading); set => SetProperty(() => IsLoading, value); }
	public ObservableCollection<BookModel> Books { get; } = [];
	#endregion

	private BookRepository()
	{
		Books.CollectionChanged += Books_CollectionChanged;
	}

	#region MyEvents
	public delegate void BookPropertyChanged(BookModel book);
	public event BookPropertyChanged OnBookChanged = default!;
	#endregion

	#region Methods
	// Public Async Methods
	public async Task InitializeAsync()
	{
		IsLoading = true;
		try
		{
			_parentIds = [.. await DBHandler.GetBookParentIdsAsync()];
			var books = (await DBHandler.GetRootBooksAsync()).Map<Book, BookModel>();
			Books.Reset(books);
			Books.ForEach(b =>
			{
				b.HasChildren = _parentIds.Contains(b.Id);
				if (b.HasChildren)
					b.Children.Add(CreatePlaceholder());
			});
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex);
			throw;
		}
		finally
		{
			IsLoading = false;
		}
	}
	public Task LoadChildrenAsync(BookModel book)
	{
		ArgumentNullException.ThrowIfNull(book);

		if (book.ChildrenLoaded)
			return Task.CompletedTask;

		if (_loadingTasks.TryGetValue(book.Id, out var existingTask))
			return existingTask;

		var task = LoadChildrenCoreAsync(book);
		_loadingTasks.Add(book.Id, task);
		return task;
	}

	// Public Methods
	public void Add(BookModel book)
	{
		ArgumentNullException.ThrowIfNull(book);

		book.Id = DBHandler.CreateBook(book.Map<Book>());
		if (book.ParentId.HasValue)
		{
			_parentIds.Add(book.ParentId.Value);
			if (book.Parent != null)
			{
				var parent = book.Parent;
				bool hadChildren = parent.HasChildren;

				parent.HasChildren = true;

				if (parent.ChildrenLoaded || !hadChildren)
				{
					parent.Children.Add(book);
					parent.ChildrenLoaded = true;
				}
			}
			return;
		}
		Books.Add(book);
	}
	public void Delete(BookModel book)
	{
		ArgumentNullException.ThrowIfNull(book);
		try
		{
			var parent = book.Parent;
			if (!DBHandler.DeleteBook(book.Id))
				return;
			if (parent != null)
			{
				if (parent.ChildrenLoaded)
				{
					parent.Children.Remove(book);

					if (parent.Children.Count == 0)
					{
						_parentIds.Remove(parent.Id);
						parent.HasChildren = false;
					}
				}
				_parentIds.Remove(book.Id);
			}
			else
				Books.Remove(book);
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex);
			MessageBox.Show("حذف دفتر با وجود فرزند امکان پذیر نیست", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
	public BookModel GetBook(int id) => _flatBooks.Find(b => b.Id == id) ?? default!;
	public IEnumerable<BookModel> GetBooksByIds(IEnumerable<int> Ids) => _flatBooks.Join(Ids, a => a.Id, b => b, (a, b) => a);
	public void CloseBooks()
	{
		var books = DBHandler.GetLeaves();
		foreach (Book book in books)
		{
			var dc = DBHandler.GetSumDC(book.Id);
			book.InitialAmount = (int)Math.Abs(((book.IsCredit ? 1 : -1) * book.InitialAmount) + dc.Item1 - dc.Item2);
		}
		DBHandler.UpdateBookChanges(books);
		DBHandler.ClearTable(ChortkeTable.Document);
		DBHandler.ClearTable(ChortkeTable.Article);
	}

	// Private Methods
	private async Task LoadChildrenCoreAsync(BookModel book)
	{
		try
		{
			var children = await DBHandler.GetBookChildrenAsync(book.Id);

			var childModels = new ObservableCollection<BookModel>(
			children.Map<Book, BookModel>((source, destination) =>
			{
				destination.Parent = book;
				destination.HasChildren = _parentIds.Contains(destination.Id);
			}));

			childModels.Where(x => x.HasChildren).ForEach(x => x.Children.Add(CreatePlaceholder()));

			book.Children = new ObservableCollection<BookModel>(childModels);
			book.HasChildren = childModels.Count > 0;
			book.ChildrenLoaded = true;
		}
		finally
		{
			_loadingTasks.Remove(book.Id);
		}
	}

	private void RegisterBook(BookModel book)
	{
		if (book.IsPlaceHolder)
			return;

		if (!_flatBooks.Contains(book))
			_flatBooks.Add(book);
		book.PropertyChanged -= Book_PropertyChanged;
		book.PropertyChanged += Book_PropertyChanged;

		// Unsubscribe from the previous Children collection, if any.
		if (_subscribedChildren.TryGetValue(book, out var oldChildren))
			oldChildren.CollectionChanged -= Children_CollectionChanged;

		// Subscribe to the current Children collection.
		book.Children.CollectionChanged -= Children_CollectionChanged;
		book.Children.CollectionChanged += Children_CollectionChanged;

		_subscribedChildren[book] = book.Children;
	}
	private void UnregisterBook(BookModel book)
	{
		if (book.IsPlaceHolder)
			return;
		book.PropertyChanged -= Book_PropertyChanged;

		if (_subscribedChildren.TryGetValue(book, out var children))
		{
			children.CollectionChanged -= Children_CollectionChanged;
			_subscribedChildren.Remove(book);
		}

		_flatBooks.Remove(book);
	}

	// Static Methods
	private static void CallActionOnEveryBook(BookModel book, Action<BookModel> action)
	{
		if (book.IsPlaceHolder)
			return;
		action(book);
		foreach (var child in book.Children)
			CallActionOnEveryBook(child, action);
	}
	private static BookModel CreatePlaceholder() => new() { IsPlaceHolder = true };
	#endregion

	#region Events
	private void Book_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (sender is not BookModel book)
			return;
		// موقعی که فرزندان یک دفتر لود می شوند نیازی نیست که به عنوان تغییر خود دفتر اعلان شود
		if (e.PropertyName == nameof(BookModel.Children))
		{
			CallActionOnEveryBook(book, RegisterBook);
			return;
		}
		OnBookChanged?.Invoke(book);
	}
	private void Books_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		if (e.OldItems != null)
		{
			foreach (BookModel book in e.OldItems)
				CallActionOnEveryBook(book, UnregisterBook);
		}

		if (e.NewItems != null)
		{
			foreach (BookModel book in e.NewItems)
				CallActionOnEveryBook(book, RegisterBook);
		}
	}
	private void Children_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		if (e.OldItems != null)
		{
			foreach (BookModel book in e.OldItems)
			{
				// Also unregister all descendants of the removed book.
				CallActionOnEveryBook(book, UnregisterBook);
			}
		}

		if (e.NewItems != null)
		{
			foreach (BookModel book in e.NewItems)
			{
				// Register the new book and its already-loaded descendants.
				CallActionOnEveryBook(book, RegisterBook);
			}
		}
	}
	#endregion
}
