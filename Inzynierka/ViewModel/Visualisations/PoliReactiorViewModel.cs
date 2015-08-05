using System.Collections.Generic;

namespace Inzynierka.ViewModel.Visualisations
{
    public class PoliReactiorViewModel : ViewModelBase, IVisualisator
    {
        public void SetValue(List<double> values)
        {
            PendulumT = values[0] * 90 / 25000.5 - 90;

            OnPropertyChanged("PendulumT");
        }

        public double PendulumT { get; set; }
    }
}