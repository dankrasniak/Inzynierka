using System.Windows;

namespace Inzynierka.View
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public ErrorWindow(string errorMessage)
        {
            InitializeComponent();
            this.TextBlock.Text = errorMessage;
        }

        private void AcceptButton(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
