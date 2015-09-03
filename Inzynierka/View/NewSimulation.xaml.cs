using Inzynierka.ViewModel;
using System.Windows;

namespace Inzynierka.View
{
    /// <summary>
    /// Interaction logic for NewSimulation.xaml
    /// </summary>
    public partial class NewSimulation : Window
    {
        public NewSimulation()
        {
            InitializeComponent();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AcceptButton(object sender, RoutedEventArgs e)
        {
            var vm = (MainWindowViewModel) this.DataContext;

            if (vm.SelectedAlgorithm == null || vm.SelectedModel == null)
            {
                var error = new ErrorWindow("Przed przejściem dalej, proszę wybrać algorytm sterujący i obiekt dynamiczny.");
                error.Show();
            }
            else
            {
                var s = new Settings(vm.SelectedAlgorithm, vm.SelectedModel);
                s.Owner = this.Owner;

                s.Show();
                this.Close();
            }
        }
    }
}
