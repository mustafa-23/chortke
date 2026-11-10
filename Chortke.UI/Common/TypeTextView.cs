using Moonlight.WPF;
using System.Windows;
using System.Windows.Controls;

namespace Chortke.Common
{
	public class TypeTextView : TextBlock
	{
		public enum TextType
		{
			None, Text, Number, Money, Date, DateAndTime
		}

		#region Dependency Properties
		public TextType Type
		{
			get { return (TextType)GetValue(TypeProperty); }
			set { SetValue(TypeProperty, value); }
		}
		public static readonly DependencyProperty TypeProperty =
			DependencyProperty.Register("Type", typeof(TextType), typeof(TypeTextView), new PropertyMetadata(TextType.None));

		public object RawText
		{
			get { return GetValue(RawTextProperty); }
			set { SetValue(RawTextProperty, value); }
		}
		public static readonly DependencyProperty RawTextProperty =
			DependencyProperty.Register("RawText", typeof(object), typeof(TypeTextView), new PropertyMetadata(null, RawTextChanged));
		#endregion
		private static void RawTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is TypeTextView ctrl)
				ctrl.SetText();
		}

		private void SetText()
		{
			switch (Type)
			{
				case TextType.None:
				case TextType.Text:
					Text = RawText?.ToString();
					break;
				case TextType.Number:
					Text = $"{RawText:N0}";
					break;
				case TextType.Money:
					Text = $"{RawText:C0}";
					break;
				case TextType.Date:
					Text = new DateTimeToPersianConverter().Convert(RawText, typeof(string), false, default!).ToString();
					break;
				case TextType.DateAndTime:
					Text = new DateTimeToPersianConverter().Convert(RawText, typeof(string), default!, default!).ToString();
					break;
			}
		}
	}
}