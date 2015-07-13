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
            Properties.Add(new Property("Horizon", 20, "Długość horyzontu", 0.00001, 20000.0));

            LoggedValues = new List<LoggedValue>();
            LoggedValues.Add(new LoggedValue(Name, "Wartość wyjściowa", true));
            LoggedValues.Add(new LoggedValue(Name, "Wartość wejściowa", true));
            //LoggedValues.Add(new LoggedValue(Name, , true));
        }

        public override IAlgorithm CreateAlgorithm(IModel model, List<Property> properties, List<LoggedValue> loggedValues)
        {
            throw new System.NotImplementedException();
            return new Advisor(model, properties, loggedValues);
        }
    }
}