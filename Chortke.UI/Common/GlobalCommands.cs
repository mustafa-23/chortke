using DevExpress.Mvvm;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Moonlight;
using System.IO;

namespace Chortke.Common
{
	public static class GlobalCommands
	{
		public static ICommand AddDocument { get; } = new DelegateCommand(() => MyModuleManager.InjectTopMost(Modules.Document));
		public static ICommand Close { get; } = new DelegateCommand(Application.Current.Shutdown);
		public static ICommand CloseBooks { get; } = new DelegateCommand(() =>
		{
			var message = "با این کار مانده تمام دفاتر تا زمان جاری محاسبه شده و به عنوان مانده اولیه تنظیم شده و همچنین دیتاها به دیتابیس جداگانه با نام سال قبل منتقل خواهند شد.\n از ادامه عملیات اظمینان دارید؟";
			if (MessageBox.Show(message, "اخطار", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
			{
				var pc = new PersianCalendar();
				var currentYear = DateTime.Now.GetShamsiYear();
				try
				{
					File.Copy("chortke.db", $"{currentYear - 1}.db");
					BookRepository.CloseBooks();
					MessageBox.Show("عملیات بستن دفاتر با موفقیت انجام شد", "پیغام", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Error in Closing Books Action", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		});
	}
}
