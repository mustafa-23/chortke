using Chortke.Common;
using Chortke.Data;
using Chortke.Models;
using DevExpress.Mvvm;
using Moonlight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Chortke.UserControls;

public partial class BookSelectControl : UserControl, INotifyPropertyChanged
{
	private List<BookModel> changedBooks = [];
	public enum Action
	{
		Add, Save, Delete
	};

	public BookModel SelectedBook
	{
		get { return (BookModel)GetValue(SelectedBookProperty); }
		set { SetValue(SelectedBookProperty, value); }
	}
	public static readonly DependencyProperty SelectedBookProperty =
		DependencyProperty.Register("SelectedBook", typeof(BookModel), typeof(BookSelectControl), new PropertyMetadata(null));

	public event PropertyChangedEventHandler? PropertyChanged;

	public BookSelectControl()
	{
		InitializeComponent();

		DataContext = this;
		Loaded += BookSelectControl_Loaded;
		Unloaded += BookSelectControl_Unloaded;
	}

	private void BookSelectControl_Unloaded(object sender, RoutedEventArgs e)
	{
		Messenger.Default.Unregister<BookSelectControlMessage>(this, ReceiveMessage);
		BookRepository.Instance.OnBookChanged -= Instance_OnBookChanged;
	}
	private void BookSelectControl_Loaded(object sender, RoutedEventArgs e)
	{
		Messenger.Default.Register<BookSelectControlMessage>(this, ReceiveMessage);
		BookRepository.Instance.OnBookChanged += Instance_OnBookChanged;
	}

	private void Instance_OnBookChanged(BookModel book)
	{
		if (!changedBooks.Contains(book))
			changedBooks.Add(book);
	}

	private void ReceiveMessage(BookSelectControlMessage message)
	{
		// all instances will receive this message but we want the one that is active right now
		if (!Equals(Tag, message.Tag)) return;
		switch (message.Action)
		{
			case Action.Add:
				AddBook();
				break;
			case Action.Save:
				if (changedBooks.Count > 0)
				{
					DBHandler.UpdateBookChanges(changedBooks.Map<BookModel, Book>());
					changedBooks.Clear();
				}
				break;
			case Action.Delete:
				if (SelectedBook != null)
					BookRepository.Instance.Delete(SelectedBook);
				SelectedBook = null;
				break;
			default:
				break;
		}
	}

	private void AddBook()
	{
		var book = new BookModel { Name = "Untitled Book", InitialAmount = 0 };
		if (Keyboard.Modifiers != ModifierKeys.Control && SelectedBook != null)
		{
			book.Parent = SelectedBook;
			book.ParentId = SelectedBook.Id;
		}
		try
		{
			BookRepository.Instance.Add(book);
			SelectedBook = book;
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex);
			MessageBox.Show("ایجاد دفتر امکان‌پذیر نیست", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}

	private async void TreeView_Expanded(object sender, RoutedEventArgs e)
	{
		if (e.OriginalSource is not TreeViewItem { DataContext: BookModel book })
			return;
		if (book.IsPlaceHolder || !book.HasChildren || book.ChildrenLoaded)
			return;

		try
		{
			await BookRepository.Instance.LoadChildrenAsync(book);
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex);
			MessageBox.Show("بارگذاری دفاتر با خطا مواجه شد.", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}

	private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		SelectedBook = treeView.SelectedItem as BookModel ?? default!;
	}
}

public class BookSelectControlMessage
{
	public BookSelectControl.Action Action { get; set; }
	public string Tag { get; set; } = default!;
}
