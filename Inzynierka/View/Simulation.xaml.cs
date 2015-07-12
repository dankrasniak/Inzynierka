using System.Windows;
using Inzynierka.ViewModel;

namespace Inzynierka.View
{
    /// <summary>
    /// Interaction logic for Simulation.xaml
    /// </summary>
    public partial class Simulation : Window
    {
        public Simulation()
        {
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            ((SimulationViewModel)this.DataContext).IsFinished = true;
            ((SimulationViewModel) this.DataContext).Loop = ((SimulationViewModel) this.DataContext).Faster = false;
            base.OnClosing(e);
            this.Owner.Focus();
        }
    }
}
