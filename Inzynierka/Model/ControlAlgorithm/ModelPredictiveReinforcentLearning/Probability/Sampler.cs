using System;

namespace Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcentLearning.Probability
{
    public interface INormalSampler
    {
        double SampleFromNormal(double mean, double std_dev);
    }

    [Serializable]
    public class ASampler : Random, INormalSampler
    {
        public double SampleFromNormal(double mean, double std_dev)
        {
            double z = -Math.Log(1.0 - NextDouble());
            double alpha = NextDouble() * Math.PI * 2;
            double norm = Math.Sqrt(z * 2) * Math.Cos(alpha);
            return mean + norm * std_dev;
            //double sum=0; 
            //for (int i=0; i<12; i++)
            //	sum += Sample(); 
            //return ((sum - 6) * std_dev + mean); 
        }
    }
}