using System;
using System.Collections.Generic;

namespace Inzynierka.Model
{
    public class Data
    {
        public List<Double> Values { get; set; }

        public int EpisodeNumber { get; set; }

        public int IterationNumber { get; set; }

        public List<Double> AdditionalValue { get; set; }
    }
}