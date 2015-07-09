using System;
using System.Collections.Generic;

namespace Inzynierka.Model.Model.Pendulum
{
    public class PendulumFactory : AModelFactory
    {
        public override string Name { get; set; }
        public override string Description { get; set; }
        public override List<Property> Properties { get; set; }

        public PendulumFactory()
        {
            Name = "Wahadło";
            Description = "Opis wahadła.";

            Properties = new List<Property>();
            Properties.Add(new Property("Setpoint", 0.0, "Wartość zadana", 0.00001, 20000.0));
            Properties.Add(new Property("S0V1", 0.0, "Wartość stanu x", 0.00001, 20000.0));
            Properties.Add(new Property("S0V2", 0.0, "Wartość stanu o", 0.00001, 20000.0));
            Properties.Add(new Property("S0V3", 0.0, "Wartość stanu xp", 0.00001, 20000.0));
            Properties.Add(new Property("S0V4", 0.0, "Wartość stanu op", 0.00001, 20000.0));
            Properties.Add(new Property("CommandingValue", 0.0, "Wartość sterująca.", 0.00001, 20000.0));
        }

        public override IModel CreateModel(List<Property> properties)
        {
            return new Pendulum(properties); // TODO
        }
    }
}