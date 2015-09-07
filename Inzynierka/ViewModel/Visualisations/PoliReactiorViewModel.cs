using System.Collections.Generic;

namespace Inzynierka.ViewModel.Visualisations
{
    public class PoliReactiorViewModel : ViewModelBase, IVisualisator
    {
        public void SetValue(List<double> values)
        {
            PendulumT = values[0] * 90 / values[2] - 90; // TODO Stała wartość
            
            OnPropertyChanged("PendulumT");

            Value90 = values[2];
            OnPropertyChanged("Value90");

            SetPointValue = values[1];
            OnPropertyChanged("SetPointValue");

            SetPointAngle = values[1] * 90 / values[2] - 90;
            OnPropertyChanged("SetPointAngle");
        }

        public double PendulumT { get; set; }
        public double Value90 { get; set; }
        public double SetPointValue { get; set; }
        public double SetPointAngle { get; set; }
    }
}