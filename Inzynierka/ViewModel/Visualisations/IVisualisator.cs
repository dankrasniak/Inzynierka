using System.Collections.Generic;

namespace Inzynierka.ViewModel.Visualisations
{
    public interface IVisualisator
    {
        void SetValue(List<double> values);
    }
}