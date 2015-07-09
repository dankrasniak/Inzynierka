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
            if (false) // TODO
            {
                if (vm.SelectedAlgorithm.Equals(null) || vm.SelectedModel.Equals(null))
                    return; // TODO NEW WINDOW("SET THE ALGORITHM / MODEL FIRST FAGGET!");
            }
            Settings s = new Settings(vm.SelectedAlgorithm, vm.SelectedModel);
            s.Owner = this.Owner;

            s.Show();
            this.Close();
        }
    }
}
