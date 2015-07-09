using System.Collections.Generic;
using Inzynierka.Model.Logger;
using Inzynierka.Model.Model;

namespace Inzynierka.Model.ControlAlgorithm.PredictionControl
{
    public sealed class PredictionControlFactory : AAlgorithmFactory
    {
        public override string Name { get; set; }
        public override string Description { get; set; }
        public override List<Property> Properties { get; set; }
        public override List<LoggedValue> LoggedValues { get; set; }

        public PredictionControlFactory()
        {
            Name = "Sterowanie Predykcyjne";
            Description = "Opis algorytmu sterowania predykcyjnego.";

            Properties = new List<Property>();
            Properties.Add(new Property("H_STEP", 0.001, "RungeKutta, wartość skoku.", 0.00001, 1.0));
            Properties.Add(new Property("Horizon", 20, "Długość horyzontu", 0.00001, 20000.0));
            Properties.Add(new Property("StartSigma", 0.001, "Algorytm ewolucyjny, wartość sigmy.", 0.001, 20.0));
            Properties.Add(new Property("M", 10, "Algorytm ewolucyjny, wartość M.", 0.001, 20.0));

            LoggedValues = new List<LoggedValue>();
            LoggedValues.Add(new LoggedValue(Name, "Wartość wyjściowa", true));
            LoggedValues.Add(new LoggedValue(Name, "stateVariables", true));
            LoggedValues.Add(new LoggedValue(Name, "horizon", true));
            LoggedValues.Add(new LoggedValue(Name, "Possible states", true));
            LoggedValues.Add(new LoggedValue(Name, "Wartość wejściowa", true));
            //LoggedValues.Add(new LoggedValue(Name, , true));
        }

        public override IAlgorithm CreateAlgorithm(IModel model, List<Property> properties, List<LoggedValue> loggedValues)
        {
            return new PredictionControl(model, properties, loggedValues); // TODO Add path for logs.
        }
    }
}