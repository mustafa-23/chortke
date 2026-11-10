using Chortke.Data;
using DevExpress.Mvvm.Native;
using Moonlight;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Chortke.Common;
using DevExpress.Mvvm.POCO;
using System.ComponentModel;

namespace Chortke.UI.ViewModels;

public class FooterViewModel : Moonlight.MVVM.ViewModelBase<FooterViewModel>
{
	private bool firstRun = true;
	private readonly ObservableCollection<string> dbfiles = [];
	public ObservableCollection<string> DBFiles
	{
		get
		{
			if (dbfiles.Count == 0)
			{
				var temp = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.db");
				dbfiles.AddRange(temp.Select(x => Path.GetFileName(x)));
				SelectedDBFile = dbfiles.FirstOrDefault(x => x.StartsWith("chortke")) ?? "chortke.db";
			}
			return dbfiles;
		}
	}

	public virtual string SelectedDBFile { get; set; } = string.Empty;

	public FooterViewModel()
	{
		(this as INotifyPropertyChanged)?.PropertyChanged += FooterViewModel_PropertyChanged;
	}

	private async void FooterViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(SelectedDBFile))
		{
			DBHandler.ChangeDb(SelectedDBFile);
			if (!firstRun)
				await BookRepository.Instance.InitializeAsync();
			firstRun = false;
		}
	}
}