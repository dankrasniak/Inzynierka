using System.Collections.Generic;

namespace Inzynierka.Model.Model.PoliReactor
{
    public class PoliReactorFactory : AModelFactory
    {
        public override string Name { get; set; }
        public override string Description { get; set; }
        public override List<Property> Properties { get; set; }

        public PoliReactorFactory()
        {
            Name = "Reaktor polimeryzacyjny";
            Description = "Opis reaktora.";

            Properties = new List<Property>();
            Properties.Add(new Property("H_STEP", 0.001, "RungeKutta, wartość skoku.", 0.00001, 1.0));
            Properties.Add(new Property("S0V1", 0.01, "Wartość 1 stanu poczatkowego reaktora.", 0.0, 1000));
            Properties.Add(new Property("S0V2", 0.01, "Wartość 2 stanu poczatkowego reaktora.", 0.0, 1000));
            Properties.Add(new Property("S0V3", 0.01, "Wartość 3 stanu poczatkowego reaktora.", 0.0, 1000));
            Properties.Add(new Property("S0V4", 0.01, "Wartość 4 stanu poczatkowego reaktora.", 0.0, 1000));
            Properties.Add(new Property("CommandingValue", 0.0, "Pierwsza generowana wartość sterująca.", 0.0, 1000));
            Properties.Add(new Property("Setpoint", 25000.5, "Wartość, do której powinna zmierzać wartość wyjściowa reaktora.", 0.0, 50000.0));
        }

        public override IModel CreateModel(List<Property> properties)
        {
            return new PoliReactor(properties);
        }
    }
}