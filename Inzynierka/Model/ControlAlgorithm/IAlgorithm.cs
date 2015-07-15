using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace Inzynierka.Model.ControlAlgorithm
{
    public interface IAlgorithm
    {
        // TODO GetValue, Constructor with argument as IModel, List<Property>, List<LoggedValue>
        Double GetValueTMP(); // TODO Should return List<Double>
    }
}