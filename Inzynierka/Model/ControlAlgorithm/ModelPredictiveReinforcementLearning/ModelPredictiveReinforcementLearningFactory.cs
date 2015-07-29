using System.Collections.Generic;
using Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcementLearning.ReinforcementPlatform.CCMPctrl;
using Inzynierka.Model.Logger;
using Inzynierka.Model.Model;

namespace Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcementLearning
{
    public class ModelPredictiveReinforcementLearningFactory : AAlgorithmFactory
    {
        public override string Name { get; set; }
        public override string Description { get; set; }
        public override List<Property> Properties { get; set; }
        public override List<LoggedValue> LoggedValues { get; set; }

        public ModelPredictiveReinforcementLearningFactory()
        {
            Name = "Uczenie maszynowe ze sterowaniem predykcyjnym.";
            Description = "Opis algorytmu uczenia maszynowego wykorzystującego sterowanie predykcyjnege.";

            Properties = new List<Property>();
            Properties.Add(new Property("Horizon", 10, "Długość horyzontu", 1, 100));
            Properties.Add(new Property("Neural Network Layers Number", 35, "Liczba warstw sieci neuronowej.", 1, 1000));
            Properties.Add(new Property("Scalar", 0.01, "Skalar stosowany przy gradiencie dodawanym do wag sieci neuronowej.", 0.0, 10.0));
            Properties.Add(new Property("Discount", 0.8, "Wartość dyskonta stosowanego przy funkcji wartości stanu.", 0.0, 1.0));
            Properties.Add(new Property("CommandingValue", 0.01, "Pierwsza generowana wartość sterująca.", 0.0, 1000));
            Properties.Add(new Property("Sigma", 0.001, "Wartość odchylenia standardowego.", 0.0, 1000));
            Properties.Add(new Property("InternalDiscretization", 0.0001, "Wartość kroku reprezentującego czas wewnątrze funkcji stanu.", 0.0, 100.0));
            Properties.Add(new Property("ExternalDiscretization", 0.005, "Wartość kroku reprezentującego czas trwania jednej wartości sterującej", 0.0, 100.0));
            Properties.Add(new Property("TimesToAdjust", 10, "Ilość razy ile ma mieć miejsce próba poprawienia wartości sterujących na horyzoncie.", 1, 1000));
            Properties.Add(new Property("TimesToTeach", 10, "Ilość razy ile ma mieć miejsce nauka sieci neuronowej, co iterację, na podstawie uzyskanej bazy wiedzy.", 1, 1000));
            Properties.Add(new Property("TimeLimit", 10, "Limit czasu na jeden epizod.", 1, 1000));

            LoggedValues = new List<LoggedValue>();
            LoggedValues.Add(new LoggedValue(Name, "Wartość wyjściowa", false));
            LoggedValues.Add(new LoggedValue(Name, "Wartość wejściowa", false));
            //LoggedValues.Add(new LoggedValue(Name, , true));
        }

        public override IAlgorithm CreateAlgorithm(IModel model, List<Property> properties, List<LoggedValue> loggedValues)
        {
            return new Advisor(model, properties, loggedValues);
        }
    }
}