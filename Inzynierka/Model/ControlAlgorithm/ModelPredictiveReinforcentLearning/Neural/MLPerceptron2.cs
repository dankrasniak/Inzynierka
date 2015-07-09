using System;
using System.Linq;
using Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcentLearning.Computing;


namespace Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcentLearning.Neural
{
    public class MLPerceptron2
    {
        protected struct ALayer
        {
            #region Activation functions
            public static void ActivIdentity(Vector x, ref Vector v)
            {
                if (v == null || v.Dimension < x.Dimension)
                    throw new Exception("Neural.MLPerceptron2.ALayer.ActivIdentity()");
                int dim = x.Dimension;
                for (int i = 0; i < dim; i++)
                    v[i] = x[i];
            }

            public static void DActivIdentity(Vector x, Vector v, ref Vector derivatives)
            {
                if (v == null || derivatives == null || v.Dimension < x.Dimension || derivatives.Dimension < x.Dimension)
                    throw new Exception("Neural.MLPerceptron2.ALayer.DActivIdentity()");
                derivatives.FillWith(1);
            }

            public static void ActivLogit(Vector x, ref Vector v)
            {
                if (v == null || v.Dimension < x.Dimension)
                    throw new Exception("Neural.MLPerceptron2.ALayer.ActivLogit()");
                int dim = x.Dimension;
                for (int i = 0; i < dim; i++)
                    v[i] = 1.0 / (1.0 + Math.Exp(-x[i]));
            }

            public static void DActivLogit(Vector x, Vector v, ref Vector derivatives)
            {
                if (v == null || derivatives == null || v.Dimension < x.Dimension || derivatives.Dimension < x.Dimension)
                    throw new Exception("Neural.MLPerceptron2.ALayer.DActivLogit()");
                int dim = x.Dimension;
                for (int i = 0; i < dim; i++)
                    derivatives[i] = v[i] * (1.0 - v[i]);
            }

            public static void ActivAtan(Vector x, ref Vector v)
            {
                if (v == null || v.Dimension < x.Dimension)
                    throw new Exception("Neural.MLPerceptron2.ALayer.ActivAtan()");
                int dim = x.Dimension;
                for (int i = 0; i < dim; i++)
                    v[i] = Math.Atan(x[i]);
            }

            public static void DActivAtan(Vector x, Vector v, ref Vector derivatives)
            {
                if (v == null || derivatives == null || v.Dimension < x.Dimension || derivatives.Dimension < x.Dimension)
                    throw new Exception("Neural.MLPerceptron2.ALayer.DActivAtan()");
                int dim = x.Dimension;
                for (int i = 0; i < dim; i++)
                    derivatives[i] = 1.0 / (1.0 + x[i] * x[i]);
            }
            #endregion

            #region Fields
            public Vector Input;
            public Vector dL_dInput;
            public Matrix Weights;
            public Matrix dL_dWeights;
            public Vector X;
            public Vector dL_dX;
            public delegate void Delegate11(Vector x, ref Vector v);
            public Delegate11 Activation;
            public delegate void Delegate12(Vector x, Vector v, ref Vector derivatives);
            public Delegate12 DActivation;
            public Vector Output;
            public Vector dOutput_dX;
            public Vector dL_dOutput;
            #endregion

            #region Build
            public void Build(int in_dim, CellType act_type, int out_dim)
            {
                Input = new Vector(in_dim + 1);
                Input[in_dim] = 1;
                dL_dInput = new Vector(in_dim + 1);
                Weights = new Matrix(out_dim, in_dim + 1);
                dL_dWeights = new Matrix(out_dim, in_dim + 1);
                X = new Vector(out_dim);
                dL_dX = new Vector(out_dim);
                switch (act_type)
                {
                    case CellType.Linear:
                        Activation = new Delegate11(ActivIdentity);
                        DActivation = new Delegate12(DActivIdentity);
                        break;
                    case CellType.Expotential:
                        Activation = new Delegate11(ActivLogit);
                        DActivation = new Delegate12(DActivLogit);
                        break;
                    case CellType.Arcustangent:
                        Activation = new Delegate11(ActivAtan);
                        DActivation = new Delegate12(DActivAtan);
                        break;
                    default:
                        throw new System.Exception(this.GetType().ToString() + ".Build(): unknown cell type");
                }
                Output = new Vector(out_dim);
                dOutput_dX = new Vector(out_dim);
                dL_dOutput = new Vector(out_dim);
            }
            #endregion

            #region Calculations
            public void CalculateForwad()
            {
                X = Weights * Input; // w C++ szybciej bedzie: X.SetProduct(Weights, Input); 
                Activation(X, ref Output);
            }

            public void CalculateBackward()
            {
                DActivation(X, Output, ref dOutput_dX);
                dL_dX = dOutput_dX & dL_dOutput; // w C++ szybciej bedzie: dL_dX.SetProducts(dOutput_dX, dL_dOutput);
                dL_dWeights = dL_dX | Input; // w C++ szybciej bedzie: dL_dWeights.SetProduct(dL_dX, Input);
                dL_dInput = dL_dX * Weights; // w C++ szybciej bedzie: dL_dInput.SetProduct(dL_dX, Weights);
            }
            #endregion
        }

        #region Data structures
        protected Random Sampler;

        public Vector InputAverage;
        public Vector InputInvStddev;
        protected Vector InSizeTab;
        protected Vector Input;

        protected ALayer[] Layer;

        protected Vector OutSizeTab;
        protected Vector dL_dOutput;

        protected Vector NetSizeTab;
        #endregion

        #region auxiliary functions
        protected double GetNormalRandomValue(double average, double std_dev)
        {
            double z = -Math.Log(1.0 - Sampler.NextDouble());
            double alpha = Sampler.NextDouble() * Math.PI * 2;
            double norm = Math.Sqrt(z * 2) * Math.Cos(alpha);
            return average + norm * std_dev;
        }
        #endregion

        #region Construction and initialization
        public MLPerceptron2()
        {
            Sampler = new Random();
        }

        public void Build(int in_dim, int[] layer_size, CellType[] types)
        {
            #region validating arguments

            if (in_dim < 0 || layer_size.Length < 1 || layer_size.Length != types.Length)
            {
                throw new Exception(this.GetType().ToString() + ".Build: wrong arguments");
            }
            if (layer_size.Any(t => t < 1))
            {
                throw new Exception(this.GetType().ToString() + ".Build: nonpositive layer size");
            }
            #endregion

            #region initializing structures

            InputAverage = new Vector(in_dim);
            InputAverage.FillWith(0);

            InputInvStddev = new Vector(in_dim);
            InputInvStddev.FillWith(1);

            Input = new Vector(in_dim);
            Layer = new ALayer[layer_size.Length];

            int layer_input_dim = in_dim;
            for (int l = 0; l < Layer.Length; l++)
            {
                Layer[l] = new ALayer();
                Layer[l].Build(layer_input_dim, types[l], layer_size[l]);
                layer_input_dim = Layer[l].Output.Dimension;
            }

            int out_dim = layer_size[layer_size.Length - 1];
            dL_dOutput = new Vector(out_dim);

            #endregion

            AfterConstruction();
        }

        virtual protected void AfterConstruction()
        {
        }

        public void Build(int in_dim, CellType type, int[] width)
        {
            int layersNr = width.Length;
            var types = new CellType[layersNr];

            int l = 0;
            for (; l < layersNr - 1; l++)
            {
                types[l] = type;
            }
            types[l] = CellType.Linear;

            Build(in_dim, width, types);
        }

        public void SetInputDescription(Vector in_av, Vector in_stddev)
        {
            int i, in_dim = Input.Dimension;
            if (in_av.Dimension != in_dim || in_stddev.Dimension != in_dim)
                throw new Exception(this.GetType().ToString() + ".SetInputDescription(...)");

            Vector.Copy(in_av, ref InputAverage);
            for (i = 0; i < in_dim; i++)
            {
                if (in_stddev[i] <= 0)
                    throw new Exception(this.GetType().ToString() + ".SetInputDescription(...): stddev nonpositive");
                InputInvStddev[i] = 1.0 / in_stddev[i];
            }
        }

        public void InitWeights(double std_dev)
        {
            for (int l = 0; l < Layer.Length - 1; l++)
            {
                var weights = Layer[l].Weights;
                for (int i = 0; i < weights.Height; i++)
                    for (int j = 0; j < weights.Width; j++)
                        weights[i, j] = GetNormalRandomValue(0, std_dev);
            }
            Layer[Layer.Length - 1].Weights.FillWith(0);
        }

        public void InitWeights()
        {
            InitWeights(1);
        }

        public void Connect2Weights(MLPerceptron2 network) // umozliwia dzialanie kilku sieci (np. w roznych watkach) z tymi samymi wagami
        {
            if (Layer.Length != network.Layer.Length)
                throw new Exception(this.GetType().ToString() + ".Weights2");
            for (int l = 0; l < Layer.Length; l++)
                if (Layer[l].Weights.Height != network.Layer[l].Weights.Height
                    || Layer[l].Weights.Width != network.Layer[l].Weights.Width)
                    throw new Exception(this.GetType().ToString() + ".Connect2Weights");
            for (int l = 0; l < Layer.Length; l++)
                Layer[l].Weights = network.Layer[l].Weights;
        }
        #endregion

        #region Properties
        public int InDimension
        {
            get { return Input.Dimension; }
        }

        public int OutDimension
        {
            get { return dL_dOutput.Dimension; }
        }

        public int WeightsCount
        {
            get
            {
                int ret = 0;
                for (int l = 0; l < Layer.Length; l++)
                    ret += Layer[l].Weights.Height * Layer[l].Weights.Width;
                return ret;
            }
        }

        public int InternalStateDimension
        {
            get
            {
                int size = 0;
                for (int l = 0; l < Layer.Length; l++)
                    size += Layer[l].X.Dimension + Layer[l].Output.Dimension;
                return size;
            }
        }

        protected Vector Output
        {
            get { return Layer[Layer.Length - 1].Output; }
        }
        #endregion

        #region Access
        public void GetWeights(ref Vector weights)
        {
            Vector.AssureDimension(ref weights, WeightsCount);
            int k = 0;
            for (int l = 0; l < Layer.Length; l++)
            {
                Matrix mat = Layer[l].Weights;
                int msize = mat.Height * mat.Width;
                MatrixToolbox.InsertMatrix2Vector(ref weights, k, mat);
                k += msize;
            }
        }

        public void SetWeights(Vector weights)
        {
            if (weights == null || weights.Dimension != WeightsCount)
                throw new Exception(this.GetType().ToString() + ".SetWeights()");
            int k = 0;
            for (int l = 0; l < Layer.Length; l++)
            {
                Matrix mat = Layer[l].Weights;
                int msize = mat.Height * mat.Width;
                mat.FillWith(0);
                MatrixToolbox.AddScaledSubVector2Matrix(ref mat, weights, k, 1);
                k += msize;
            }
        }

        public void AddToWeights(Vector vect, double scalar)
        {
            if (vect == null || vect.Dimension != WeightsCount)
                throw new Exception(this.GetType().ToString() + ".AddToWeights()");
            int k = 0;
            for (int l = 0; l < Layer.Length; l++)
            {
                Matrix mat = Layer[l].Weights;
                int msize = mat.Height * mat.Width;
                MatrixToolbox.AddScaledSubVector2Matrix(ref mat, vect, k, scalar);
                k += msize;
            }
        }
        #endregion

        #region protected calcuations
        protected void SetInput(Vector arguments)
        {
            if (arguments == null || arguments.Dimension != Input.Dimension)
                throw new Exception(this.GetType().ToString() + ".SetInput");
            Input = arguments - InputAverage; // w C++ szybciej bedzie: Input.SetDifference(arguments, InputAverage); 
            Input = Input & InputInvStddev; // w C++ szybciej bedzie: Input.SetProducts(Input, InputInvStddev);
        }

        protected void CalculateAhead()
        {
            Vector input = Input;
            for (int l = 0; l < Layer.Length; l++)
            {
                Layer[l].Input.InsertPart(0, input.Dimension, input, 0);
                Layer[l].CalculateForwad();
                input = Layer[l].Output;
            }
        }

        protected void BackPropagateGradient()
        {
            Vector dl_doutput = dL_dOutput;
            for (int l = Layer.Length - 1; l >= 0; l--)
            {
                Layer[l].dL_dOutput.InsertPart(0, Layer[l].Output.Dimension, dl_doutput, 0);
                Layer[l].CalculateBackward();
                dl_doutput = Layer[l].dL_dInput;
            }
        }
        #endregion

        #region public calculations
        public void Approximate(Vector input, ref Vector output)
        {
            SetInput(input);
            CalculateAhead();
            Vector.AssureDimension(ref output, OutDimension);
            output.InsertPart(0, OutDimension, Layer[Layer.Length - 1].Output, 0);
        }

        public void BackPropagateGradient(Vector out_gradient, ref Vector weight_gradient)
        {
            int out_dim = this.OutDimension;
            if (out_gradient.Dimension != out_dim)
                throw new Exception(this.GetType().ToString() + ".BackPropagateGradient");
            if (dL_dOutput != out_gradient)
                dL_dOutput.InsertPart(0, out_dim, out_gradient, 0);
            BackPropagateGradient();
            Vector.AssureDimension(ref weight_gradient, WeightsCount);
            int k = 0;
            for (int l = 0; l < Layer.Length; l++)
            {
                int size = Layer[l].dL_dWeights.Height * Layer[l].dL_dWeights.Width;
                MatrixToolbox.InsertMatrix2Vector(ref weight_gradient, k, Layer[l].dL_dWeights);
                k += size;
            }
        }
        #endregion

        #region saving and restoring
        public void SaveForwardState(ref Vector state)
        {
            int offset = 0;
            Vector.AssureDimension(ref state, InternalStateDimension);
            for (int l = 0; l < Layer.Length; l++)
            {
                state.InsertPart(offset, Layer[l].X.Dimension, Layer[l].X, 0);
                offset += Layer[l].X.Dimension;
                state.InsertPart(offset, Layer[l].Output.Dimension, Layer[l].Output, 0);
                offset += Layer[l].Output.Dimension;
            }
        }

        public void RestoreForwardState(Vector state)
        {
            int offset = 0;
            if (state.Dimension != InternalStateDimension)
                throw new Exception(GetType().ToString() + ".RestoreForwardState");
            for (int l = 0; l < Layer.Length; l++)
            {
                Layer[l].X.InsertPart(0, Layer[l].X.Dimension, state, offset);
                offset += Layer[l].X.Dimension;
                Layer[l].Output.InsertPart(0, Layer[l].Output.Dimension, state, offset);
                offset += Layer[l].Output.Dimension;
            }
        }
        #endregion
    }
}