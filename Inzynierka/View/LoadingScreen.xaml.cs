using System;
using System.Threading.Tasks;
using System.Windows;

namespace Inzynierka.View
{
    /// <summary>
    /// Interaction logic for LoadingScreen.xaml
    /// </summary>
    public partial class LoadingScreen : Window
    {
        public LoadingScreen()
        {
            InitializeComponent();
            MyClose();
        }

        private async void MyClose()
        {
            MainWindow mw = new MainWindow();
            for (Double i = 2.0; i >= 0; i = i - 0.02)
            {
                this.Opacity = i;
                await Task.Delay(10);
            }
            mw.Show();
            this.Close();
        }
    }
}
