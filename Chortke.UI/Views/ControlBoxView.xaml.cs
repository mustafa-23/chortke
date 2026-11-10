using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Chortke.UI.Views
{
    public partial class ControlBoxView : UserControl
    {
        public ControlBoxView()
        {
            InitializeComponent();
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.MainWindow.DragMove();
        }
    }
}
