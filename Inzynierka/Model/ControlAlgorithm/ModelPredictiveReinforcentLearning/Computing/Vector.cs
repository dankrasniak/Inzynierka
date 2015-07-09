using System;

namespace Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcentLearning.Computing
{
    public interface IVector
    {
        int Dimension
        {
            get;
            set;
        }

        double this[int i]
        {
            get;
            set;
        }

        double NormEuclideanSq
        {
            get;
        }

        double NormEuclidean
        {
            get;
        }

        double[] Table
        {
            get;
        }
    }
    /// <summary>
    /// Summary description for Vector.
    /// </summary>
    [Serializable]
    public class Vector : IVector
    {
        private double[] Numbers;

        #region Constructors
        public Vector()
        {
        }

        public Vector(int dimension)
        {
            Numbers = new double[dimension];
        }

        public Vector(int dimension, double init_val)
            : this(dimension)
        {
            FillWith(init_val);
        }

        public Vector(double[] numbers)
            : this(numbers.GetLength(0))
        {
            for (int i = Dimension - 1; i >= 0; i--)
                Numbers[i] = numbers[i];
        }

        public static void AssureDimension(ref Vector v, int dimension)
        {
            if (v == null || v.Dimension != dimension)
                v = new Vector(dimension);
        }

        public Vector Clone()
        {
            Vector ret = new Vector(Dimension);
            for (int i = 0; i < Dimension; i++)
                ret.Numbers[i] = Numbers[i];
            return (ret);
        }

        public static void Copy(IVector pattern, ref Vector v)
        {
            int dim = pattern.Dimension;
            AssureDimension(ref v, dim);
            for (int i = 0; i < dim; i++)
                v.Numbers[i] = pattern[i];
        }

        public static void Copy(IVector pattern, ref double[] array)
        {
            int dim = pattern.Dimension;
            if (array == null || array.Length != dim)
                array = new double[dim];
            for (int i = 0; i < dim; i++)
                array[i] = pattern[i];
        }

        public static void Copy(double[] pattern, ref Vector v)
        {
            int dim = pattern.Length;
            AssureDimension(ref v, dim);
            for (int i = 0; i < dim; i++)
                v.Numbers[i] = pattern[i];
        }

        public static void Copy(double[] pattern, ref double[] array)
        {
            int dim = pattern.Length;
            if (array == null || array.Length != dim)
                array = new double[dim];
            for (int i = 0; i < dim; i++)
                array[i] = pattern[i];
        }
        #endregion

        #region Access
        public double[] Table
        {
            get { return Numbers; }
        }

        public double this[int i]
        {
            get { return (Numbers[i]); }
            set { Numbers[i] = value; }
        }

        public int Dimension
        {
            get
            {
                return (Numbers == null) ? 0 : (Numbers.GetLength(0));
            }
            set
            {
                if (Numbers == null || Numbers.GetLength(0) != value)
                    Numbers = new double[value];
            }
        }

        public void Insert(int position, Vector v)
        {
            Insert(position, v.Numbers);
        }

        public void Insert(int position, double[] numbers)
        {
            if (position + numbers.Length > Dimension)
                throw new Exception(this.GetType().ToString() + ".Insert(int,Vector)");

            for (int i = 0; i < numbers.Length; i++)
                Numbers[position + i] = numbers[i];
        }

        public void InsertPart(int this_offset, int length, Vector v, int v_offset)
        {
            if (this_offset + length > Dimension || v_offset + length > v.Dimension)
                throw new Exception(this.GetType().ToString() + ".InsertPart()");
            for (int i = 0; i < length; i++)
                this[this_offset + i] = v[v_offset + i];
        }

        public Vector Subvector(int position, int dimension)
        {
            Vector ret = new Vector(dimension);
            for (int i = 0; i < dimension; i++)
                ret[i] = Numbers[position + i];
            return (ret);
        }

        public void FillWith(double val)
        {
            for (int i = Dimension - 1; i >= 0; i--)
                Numbers[i] = val;
        }

        public double NormEuclideanSq
        {
            get
            {
                double s = 0;
                for (int i = 0; i < Numbers.GetLength(0); i++)
                    s += Numbers[i] * Numbers[i];
                return s;
            }
        }

        public double NormEuclidean
        {
            get { return Math.Sqrt(NormEuclideanSq); }
        }
        #endregion

        #region Arithmetic operators
        public static Vector operator -(Vector v)
        {
            int dim = v.Dimension;
            Vector ret = new Vector(dim);
            for (int i = 0; i < dim; i++)
                ret[i] = -v[i];
            return (ret);
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            int dim = v1.Dimension;
            if (dim != v2.Dimension)
                throw (new Exception("Vector.operator+() wrong vectors dimensions"));
            Vector ret = new Vector(dim);
            for (int i = 0; i < dim; i++)
                ret[i] = v1[i] + v2[i];
            return (ret);
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            int dim = v1.Dimension;
            if (dim != v2.Dimension)
                throw (new Exception("Vector.operator-() wrong vectors dimensions"));
            Vector ret = new Vector(dim);
            for (int i = 0; i < dim; i++)
                ret[i] = v1[i] - v2[i];
            return (ret);
        }

        public static Vector operator *(Vector v, double factor)
        {
            int dim = v.Dimension;
            Vector ret = new Vector(dim);
            for (int i = 0; i < dim; i++)
                ret[i] = v[i] * factor;
            return (ret);
        }

        public static Vector operator /(Vector v, double divisor)
        {
            int dim = v.Dimension;
            Vector ret = new Vector(dim);
            for (int i = 0; i < dim; i++)
                ret[i] = v[i] / divisor;
            return (ret);
        }

        public void Add(IVector v, double factor)
        {
            int dim = Dimension;
            if (dim != v.Dimension)
                throw new Exception("Dimensions do not match");
            for (int i = 0; i < dim; i++)
                Numbers[i] += v[i] * factor;
        }

        public void MultiplyBy(double factor)
        {
            int dim = Dimension;
            for (int i = 0; i < dim; i++)
                Numbers[i] *= factor;
        }

        //	scalar multiplication of vectors
        public static double operator *(Vector v1, Vector v2)
        {
            int dim = v1.Dimension;
            if (dim != v2.Dimension)
                throw (new System.Exception("Multiplication of two vectors is impossible due to wrong dimensions"));
            double ret = 0;
            for (int i = 0; i < dim; i++)
                ret += v1[i] * v2[i];
            return (ret);
        }

        // product of vector and matrix 
        public static Vector operator *(Vector v, Matrix m)
        {
            int rows = m.Height;
            int cols = m.Width;
            if (v.Dimension != rows)
                throw new Exception("Computing.Vector.operator*(Vector,Matrix)");
            Vector ret = new Vector(cols);
            ret.FillWith(0);
            for (int j = 0; j < cols; j++)
                for (int i = 0; i < rows; i++)
                    ret[j] += m[i, j] * v[i];
            return (ret);
        }

        //	multiplication of vectors element by element 
        public static Vector operator &(Vector v1, Vector v2)
        {
            int dim = v1.Dimension;
            if (dim != v2.Dimension)
                throw (new Exception("Vector.operator&() wrong vectors dimensions"));
            Vector ret = new Vector(dim);
            for (int i = 0; i < dim; i++)
                ret[i] = v1[i] * v2[i];
            return (ret);
        }

        //	multiplication matrix = v1*v2^T 
        public static Matrix operator |(Vector v1, Vector v2)
        {
            int dim1 = v1.Dimension, dim2 = v2.Dimension;
            Matrix ret = new Matrix(dim1, dim2);
            for (int i = 0; i < dim1; i++)
                for (int j = 0; j < dim2; j++)
                    ret[i, j] = v1[i] * v2[j];
            return (ret);
        }
        #endregion

        #region Setting values
        //	multiplication of vectors element by element 
        public void SetProducts(Vector v1, Vector v2)
        {
            int dim = v1.Dimension;
            if (dim != v2.Dimension)
                throw (new Exception("Vector.operator&() wrong vectors dimensions"));
            Dimension = dim;
            for (int i = 0; i < dim; i++)
                this[i] = v1[i] * v2[i];
        }

        //  matrix * vector 
        public void SetProduct(Matrix m, Vector v)
        {
            int rows = m.Height;
            int cols = m.Width;
            if (v.Dimension != cols)
                throw new Exception(this.GetType().ToString() + ".SetProduct(Matrix,Vector)");
            Dimension = rows;
            for (int i = 0; i < rows; i++)
            {
                double ret = 0;
                for (int j = 0; j < cols; j++)
                    ret += m[i, j] * v[j];
                this[i] = ret;
            }
        }

        //  matrix * vector 
        public void SetProduct(Vector v, Matrix m)
        {
            int rows = m.Height;
            int cols = m.Width;
            if (v.Dimension != cols)
                throw new Exception(this.GetType().ToString() + ".SetProduct(Matrix,Vector)");
            Dimension = rows;
            for (int i = 0; i < rows; i++)
            {
                double ret = 0;
                for (int j = 0; j < cols; j++)
                    ret += m[i, j] * v[j];
                this[i] = ret;
            }
        }
        #endregion
    }
}