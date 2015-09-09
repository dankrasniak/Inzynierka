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
            Properties.Add(new Property("Horizon", 2, "Długość horyzontu", 0.00001, 20000.0));
            Properties.Add(new Property("PredictionHorizon", 0, "Długość horyzontu predykcji", 0.00001, 20000.0)); // TODO
            Properties.Add(new Property("Discount", 0.95, "Wartość dyskonta dla wartości nagród na horyzoncie.", 0.01, 1.0));
            Properties.Add(new Property("StartSigma", 0.001, "Algorytm ewolucyjny, wartość sigmy.", 0.001, 20.0));
            Properties.Add(new Property("SigmaMin", 0.0001, "Wartość minimalna sigmy.", 0.00001, 10.0));
            Properties.Add(new Property("M", 10, "Algorytm ewolucyjny, wartość M.", 0.001, 20.0));
            Properties.Add(new Property("InternalDiscretization", 0.001, "Wartość kroku reprezentującego czas wewnątrze funkcji stanu.", 0.0, 100.0));
            Properties.Add(new Property("ExternalDiscretization", 0.01, "Wartość kroku reprezentującego czas trwania jednej wartości sterującej", 0.0, 100.0));
            Properties.Add(new Property("TimeLimit", 0.2, "Limit czasu na jeden epizod.", 1, 1000));
            Properties.Add(new Property("OptimisationIterationLimit", 100, "Limit iteracji na jedną akcję opltyamlizacyjną.", 1, 1000));

            LoggedValues = new List<LoggedValue>
            {
                new LoggedValue(Name, "Wartość wyjściowa", false),
                new LoggedValue(Name, "stateVariables", false),
                new LoggedValue(Name, "horizon", false),
                new LoggedValue(Name, "Possible states", false),
                new LoggedValue(Name, "Wartość wejściowa", false),
                new LoggedValue(Name, "Średnie wartości nagród z 10 epizodów", false)
            };
        }

        public override IAlgorithm CreateAlgorithm(IModel model, List<Property> properties, List<LoggedValue> loggedValues)
        {
            return new PredictionControl(model, properties, loggedValues); // TODO Add path for logs.
        }
    }
}