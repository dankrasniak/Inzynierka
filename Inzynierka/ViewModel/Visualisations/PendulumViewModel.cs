using System;
using System.Collections.Generic;
using Inzynierka.Model.Model.Pendulum;

namespace Inzynierka.ViewModel.Visualisations
{
    public class PendulumViewModel : ViewModelBase, IVisualisator
    {
        public void SetValue(List<double> values)
        {
            PendulumX = values[0] * 200 / Pendulum._XMAX + 325.0; 
            PendulumT = values[1] * 180 / Math.PI;

            OnPropertyChanged("PendulumX");
            OnPropertyChanged("PendulumT");
        }

        public double PendulumX { get; set; }

        public double PendulumT { get; set; }

        public PendulumViewModel()
        {
            PendulumX = 325.0;
        }

    }
}