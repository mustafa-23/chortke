using Chortke.Common;
using System;
using System.Windows;

namespace Chortke.UI
{
    public partial class Shell : Window
    {
        public Shell()
        {
            InitializeComponent();
            MyModuleManager.Instance.PropertyChanged += Instance_PropertyChanged;
            MaxHeight = SystemParameters.WorkArea.Height;
        }

		private void Instance_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName ==  nameof(MyModuleManager.TopmostState))
                VisualStateManager.GoToElementState(this, MyModuleManager.Instance.TopmostState, true);
		}
    }
}
