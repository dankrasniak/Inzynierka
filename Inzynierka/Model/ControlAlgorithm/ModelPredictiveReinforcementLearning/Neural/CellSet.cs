using System;
using System.Linq;
using Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcementLearning.Probability;

namespace Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcementLearning.Neural
{
    public enum CellType
    {
        Linear, Expotential, Arcustangent
    }

    [Serializable]
    public class Can	//	eng. can = pol. puszka
    {
        public double Value;
    }

    /// <summary>
    /// Summary description CellSet
    /// </summary>
    [Serializable]
    public class CellSet
    {
        [Serializable]
        protected class Cell
        {
            #region Components
            public Can[] Input;
            public double X;			//	X = Sum[i] {Input[i]*Weight[i]}
            public Can Output;		//	Output = Function(X)

            public Can[] Weight;
            public Can dL_dX;
            public Can[] dL_dWeight;

            public Can[] dL_dnextX;
            public Can[] dnextX_dOutput;

            public CellType Type;
            private delegate double Delegate11(double x);
            private Delegate11 Function;
            private delegate double Delegate12(double x, double y);
            private Delegate12 Derivative;
            #endregion

            #region Initialization
            public Cell()
            {
            }
            #endregion

            #region Neuron functions
            public static double FunctionLinear(double x)
            {
                return (x);
            }

            public static double DerivativeLinear(double x, double y)
            {
                return (1);
            }

            public static double FunctionExpotential(double x)
            {
                return (1.0 / (1.0 + System.Math.Exp(-x)));
            }

            public static double DerivativeExpotential(double x, double y)
            {
                return (y * (1.0 - y));
            }

            public static double FunctionArcustangent(double x)
            {
                return (System.Math.Atan(x));
            }

            public static double DerivativeArcustangent(double x, double y)
            {
                return (1.0 / (1.0 + x * x));
            }
            #endregion

            #region Initialization
            public void Init(int input_dim, CellType typ, int out_dim)
            {
                if (input_dim < 1 || out_dim < 1)
                    throw (new System.Exception("Neural.CellSet.Cell.Init: wrong dimension"));
                switch (typ)
                {
                    case CellType.Linear:
                        Function = new Delegate11(FunctionLinear);
                        Derivative = new Delegate12(DerivativeLinear);
                        break;
                    case CellType.Expotential:
                        Function = new Delegate11(FunctionExpotential);
                        Derivative = new Delegate12(DerivativeExpotential);
                        break;
                    case CellType.Arcustangent:
                        Function = new Delegate11(FunctionArcustangent);
                        Derivative = new Delegate12(DerivativeArcustangent);
                        break;
                    default:
                        throw new System.Exception("Neural.CellSet.Cell.Init: wrong cell type");
                }
                Type = typ;

                Input = new Can[input_dim];
                Output = new Can();
                Weight = new Can[input_dim];
                dL_dX = new Can();
                dL_dWeight = new Can[input_dim];
                for (int i = 0; i < input_dim; i++)
                {
                    Weight[i] = new Can();
                    dL_dWeight[i] = new Can();
                }

                dL_dnextX = new Can[out_dim];
                dnextX_dOutput = new Can[out_dim];
            }

            public void AddInputDimension(int index)
            {
                Input = Input.Where((e, i) => i < index).Concat(Enumerable.Repeat(new Can(), 1)).Concat(Input.Where((e, i) => i >= index)).ToArray();
                Weight = Weight.Where((e, i) => i < index).Concat(Enumerable.Repeat(new Can(), 1)).Concat(Weight.Where((e, i) => i >= index)).ToArray();
                dL_dWeight = dL_dWeight.Where((e, i) => i < index).Concat(Enumerable.Repeat(new Can(), 1)).Concat(dL_dWeight.Where((e, i) => i >= index)).ToArray();

                Weight[index].Value = 0.01;
            }

            public void RemoveInputDimension(int index)
            {
                Input = Input.Where((e, i) => i != index).ToArray();
                Weight = Weight.Where((e, i) => i != index).ToArray();
                dL_dWeight = dL_dWeight.Where((e, i) => i != index).ToArray();
            }

            public void ConnectBackward(Can[] inputs)
            {
                int in_dim = Input.Length;
                if (in_dim != inputs.Length)
                    throw new Exception("Neural.CellSet.Cell.ConnectBackward: wrong input dimension");
                for (int i = 0; i < in_dim; i++)
                    Input[i] = inputs[i];
            }

            public void ConnectForward(Can[] _dL_dnextX, Can[] _dnextX_dOutput)
            {
                int out_dim = dL_dnextX.Length;
                if (_dL_dnextX.Length != out_dim || _dnextX_dOutput.Length != out_dim)
                    throw new Exception("Neural.CellSet.Cell.ConnectForward: wrong output dimension");
                for (int i = 0; i < out_dim; i++)
                {
                    dL_dnextX[i] = _dL_dnextX[i];
                    dnextX_dOutput[i] = _dnextX_dOutput[i];
                }
            }

            public void InitWeights(INormalSampler rand, double factor)
            {
                int dim = Weight.GetLength(0);
                double std_dev = factor;//*System.Math.Sqrt((double)dim); 
                for (int i = 0; i < dim; i++)
                    Weight[i].Value = rand.SampleFromNormal(0, std_dev);
            }
            #endregion

            #region Calculations
            public void CalculateAhead()
            {
                X = 0;
                for (int i = 0; i < Weight.GetLength(0); i++)
                    X += Weight[i].Value * Input[i].Value;
                Output.Value = Function(X);
            }

            public void BackPropagateGradient()
            {
                int i;

                //  calculations:
                //              dL      dL   dOutput         dL    d_nextX  dOutput
                //              -- = ------- ------- = Sum[------- -------] -------
                //              dX   dOutput    dX         d_nextX dOutput     dX
                //  d_nextX
                //  -------  are weights of neurons in next layers
                //  dOutput  which is not visible here

                double dL_dOutput = 0;
                for (i = 0; i < dL_dnextX.GetLength(0); i++)
                    dL_dOutput += dL_dnextX[i].Value * dnextX_dOutput[i].Value;

                dL_dX.Value = dL_dOutput * Derivative(X, Output.Value);

                //  calculations:
                //              dQ   dQ dX   dQ
                //              -- = -- -- = -- Input
                //              dW   dX dW   dX
                //  Input - output of the previous layer

                for (i = 0; i < Weight.GetLength(0); i++)
                    dL_dWeight[i].Value = dL_dX.Value * Input[i].Value;
            }
            #endregion
        }

        #region Components
        protected ASampler TheSampler;
        #endregion

        #region Initialization
        public CellSet()
        {
            TheSampler = new ASampler();
        }
        #endregion
    }
}