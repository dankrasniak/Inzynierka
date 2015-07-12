using System.Windows;

namespace Inzynierka.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NewSimulation(object sender, RoutedEventArgs e)
        {
            NewSimulation ns = new NewSimulation();
            ns.DataContext = this.DataContext;
            ns.Owner = this;
            ns.Show();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            foreach (Window el in OwnedWindows)
            {
                el.Close();
            }
            base.OnClosing(e);
        }
    }
}
