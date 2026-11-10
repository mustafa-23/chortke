using Chortke.Common;
using Chortke.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Moonlight;

namespace Chortke.UI.Common
{
	[ValueConversion(typeof(int), typeof(string))]
	public class IDToTitleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int id && id > 0)
			{
				return BookRepository.Instance.GetBook(id).Name;
			}
			throw new ArgumentException($"'{value}' is not valid id");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	[ValueConversion(typeof(DateTime), typeof(string))]
	public class DateToPartConverter : IValueConverter
	{
		public string PartName { get; set; } = "month";
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			if (value is DateTime date)
			{
				if (PartName.Equals("year", StringComparison.OrdinalIgnoreCase))
					return date.GetShamsiYear();
				return date.GetShamsiMonthName();
			}
			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	[ValueConversion(typeof(object), typeof(int))]
	public class ArticlesDCConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is IEnumerable<object> models)
			{
				if (parameter is string str && str == "Credit")
					return models.OfType<ArticleModel>().Sum(x => x.IsCredit ? x.Amount : 0);
				else
					return models.OfType<ArticleModel>().Sum(x => x.IsDebit ? x.Amount : 0);
			}
			return -1;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class IsExpandedConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var now = DateTime.Now;
			if (value is string month && month == now.GetShamsiMonthName()
				|| value is int year && year == now.GetShamsiYear())
				return true;
			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
