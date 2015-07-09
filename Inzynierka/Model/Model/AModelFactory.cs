using System.Collections.Generic;

namespace Inzynierka.Model.Model
{
    public abstract class AModelFactory
    {
        public abstract string Name { get; set; }
        public abstract string Description { get; set; }
        public abstract List<Property> Properties { get; set; }
        public abstract IModel CreateModel(List<Property> properties);
    }
}