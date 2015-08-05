using System.Collections.Generic;
using System.Windows.Documents;

namespace Inzynierka.ViewModel.Visualisations
{
    public interface IVisualisator
    {
        void SetValue(List<double> values);
    }
}