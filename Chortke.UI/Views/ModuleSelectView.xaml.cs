using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Chortke.UI.Views
{
	public partial class ModuleSelectView : UserControl
	{
		public ModuleSelectView()
		{
			InitializeComponent();
		}

		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			if (sender is RadioButton radio && border != null)
			{
				Point relativePoint = radio.TransformToAncestor(grid)
							  .Transform(new Point(0, 0));
				var widthAnimation = new DoubleAnimation(radio.ActualWidth, TimeSpan.FromMilliseconds(300));
				Storyboard.SetTarget(widthAnimation, border);
				Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(Border.WidthProperty));

				var leftAnimation = new DoubleAnimation(relativePoint.X, TimeSpan.FromMilliseconds(300));
				leftAnimation.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
				Storyboard.SetTarget(leftAnimation, border);
				Storyboard.SetTargetProperty(leftAnimation, new PropertyPath(Canvas.LeftProperty));

				var story = new Storyboard();
				story.Children.Add(widthAnimation);
				story.Children.Add(leftAnimation);
				story.Begin();
			}
		}
	}
}
