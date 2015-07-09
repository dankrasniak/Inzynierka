using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
