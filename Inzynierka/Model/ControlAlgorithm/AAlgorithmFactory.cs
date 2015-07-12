using Inzynierka.Model.Logger;
using Inzynierka.Model.Model;
using System.Collections.Generic;

namespace Inzynierka.Model.ControlAlgorithm
{
    public abstract class AAlgorithmFactory
    {
        public abstract string Name { get; set; }
        public abstract string Description { get; set; }
        public abstract List<Property> Properties { get; set; }
        public abstract List<LoggedValue> LoggedValues { get; set; }
        public abstract IAlgorithm CreateAlgorithm(IModel model, List<Property> properties, List<LoggedValue> loggedValues);
    }
}