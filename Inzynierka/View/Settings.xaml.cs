using System.Linq;
using Inzynierka.Model;
using Inzynierka.Model.ControlAlgorithm;
using Inzynierka.Model.Model;
using Inzynierka.ViewModel;
using System.Windows;

namespace Inzynierka.View
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings(AAlgorithmFactory algorithmFactory, AModelFactory modelFactory)
        {
            InitializeComponent();
            var vm = (SettingWindowViewModel) this.DataContext;
            vm.AlgorithmFactory = algorithmFactory;
            vm.ModelFactory = modelFactory;
        }

        private void CancelButton(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            this.Owner.Focus();
        }

        private void AcceptButton(object sender, RoutedEventArgs e)
        {
            var dc = (SettingWindowViewModel) this.DataContext;

            // TODO
            string errorMessage = "";
            errorMessage += dc.PropertiesA.Aggregate(errorMessage, (current, property) => current + property.Verify());
            errorMessage += dc.PropertiesM.Aggregate(errorMessage, (current, property) => current + property.Verify());
            if (errorMessage.Length > 0)
            {
// ReSharper disable once ObjectCreationAsStatement
                new ErrorWindow(errorMessage);
                return;
            }

            // TODO
            // NEW SIMULATION
            var simulation = new Simulation {Owner = this.Owner};
            var simulationDataContext = (SimulationViewModel) simulation.DataContext;
            simulationDataContext.Model = dc.ModelFactory.CreateModel(dc.PropertiesM);
            simulationDataContext.Algorithm = dc.AlgorithmFactory.CreateAlgorithm(simulationDataContext.Model,
                dc.PropertiesA, dc.Log);

            simulation.Show();
            this.Close();
        }
    }
}
