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
            Properties.Add(new Property("Horizon", 20, "Długość horyzontu", 0.00001, 20000.0));
            Properties.Add(new Property("StartSigma", 0.001, "Algorytm ewolucyjny, wartość sigmy.", 0.001, 20.0));
            Properties.Add(new Property("SigmaMin", 0.001, "Wartość minimalna sigmy.", 0.00001, 10.0));
            Properties.Add(new Property("M", 10, "Algorytm ewolucyjny, wartość M.", 0.001, 20.0));

            LoggedValues = new List<LoggedValue>();
            LoggedValues.Add(new LoggedValue(Name, "Wartość wyjściowa", false));
            LoggedValues.Add(new LoggedValue(Name, "stateVariables", false));
            LoggedValues.Add(new LoggedValue(Name, "horizon", false));
            LoggedValues.Add(new LoggedValue(Name, "Possible states", false));
            LoggedValues.Add(new LoggedValue(Name, "Wartość wejściowa", false));
            //LoggedValues.Add(new LoggedValue(Name, , true));
        }

        public override IAlgorithm CreateAlgorithm(IModel model, List<Property> properties, List<LoggedValue> loggedValues)
        {
            return new PredictionControl(model, properties, loggedValues); // TODO Add path for logs.
        }
    }
}