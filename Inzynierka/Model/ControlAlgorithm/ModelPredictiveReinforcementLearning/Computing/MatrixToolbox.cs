using System;

namespace Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcementLearning.Computing
{
	/// <summary>
	/// Summary description for MatrixToolbox.
	/// </summary>
	public class MatrixToolbox
	{
		#region Empty construction
		protected MatrixToolbox()
		{
		}
		#endregion

		#region Sq and suchlike
		protected static double Sq(double a)
		{
			return (a*a); 
		}

		protected class ASampler : Random
		{
			public ASampler()
			{
			}

			public double SampleFromNormal()
			{
				double z=-Math.Log(1.0-NextDouble());
				double alpha=NextDouble()*Math.PI*2;
				return Math.Sqrt(z*2)*Math.Cos(alpha);
			}

			public void FillWithNormals(ref Vector v)
			{
				for (int i=0; i<v.Dimension; i++)
					v[i] = SampleFromNormal(); 
			}
		}
		#endregion

		#region Matrix algebra
		public static void LinearEquationGaussElimination(Matrix A, ref Vector x, Vector b)
		{
			int dim = A.Height, i,j,k; 
			if (dim!=A.Width || dim!=b.Dimension)
				throw (new System.Exception("Matrix.LinearEquationGaussSafe()  wrong dimensions")); 

			Matrix localA = A.Clone(); 
			Vector localB = b.Clone(); 

			for (i=0; i<dim; i++)
			{
				//	znalezienie wiersza zaczynajacego sie od najwiekszej wartosci
				double maxv = System.Math.Abs(localA[i,i]); 
				int indexof_max = i; 
				for (j=i+1; j<dim; j++)
					if (System.Math.Abs(localA[j,i])>maxv)
					{
						maxv = System.Math.Abs(localA[j,i]); 
						indexof_max = j; 
					}
				maxv = localA[indexof_max, i]; 
				if (maxv==0)
					throw (new System.Exception("No defined solution of the linear equation")); 
				//	wymiana wiersza i-tego z tym zaczynajacym sie od najwiekszej wartosci
				double swap = localB[indexof_max]; 
				localB[indexof_max] = localB[i]; 
				localB[i] = swap/maxv; 
				for (k=i; k<dim; k++)
				{
					swap = localA[indexof_max, k]; 
					localA[indexof_max, k] = localA[i, k]; 
					localA[i, k] = swap/maxv; 
				}
				//	poprawienie najnizszych wierszy
				for (j=i+1; j<dim; j++)
				{
					double factor = localA[j, i]; 
					localB[j] -= localB[i]*factor; 
					for (k=i; k<dim; k++)
						localA[j, k] -= localA[i, k]*factor; 
				}
			}

			if (x==null || x.Dimension!=dim)
				x = new Vector(dim); 
			for (i=dim-1; i>=0; i--)
			{
				double tmpx=localB[i]; 
				for (j=i+1; j<dim; j++)
					tmpx -= localA[i, j]*x[j]; 
				x[i] = tmpx; 
			}
		}

		public static void CholeskyFactorisation(Matrix A, ref Matrix L)
		{
			int dim = A.Height; 
			if (dim!=A.Width)
				throw (new System.Exception("Wrong matrix dimensions")); 
			if (L==null || L.Height!=dim || L.Width!=dim)
				L = new Matrix(dim, dim); 
			L.FillWith(0); 
			for (int i=0; i<dim; i++)
			{
				double lii2 = A[i,i]; 
				for (int k=0; k<i; k++)
					lii2 -= Sq(L[i,k]); 
				if (lii2<=0)
					throw (new System.Exception("Matrix has not full rank")); 
				double lii = L[i,i] = Math.Sqrt(lii2); 
				for (int j=i+1; j<dim; j++)
				{
					double sum = A[j,i]; 
					for (int k=0; k<i; k++)
						sum -= L[j,k]*L[i,k]; 
					L[j,i] = sum/lii; 
				}
			}
		}

		public static void LinearEquationAfterCholeskyF(Matrix L, ref Vector x, Vector b)
		{
			/*
				problem: rozwiazac r-nie postaci A*x = b
				Dane: macierz trojkatna dolna L (jestesmy w niej) taka ze L*L^T=A
				rozwiazanie:
				1) rozwiazac r-nie:  L * y = b
				2) rozwiazac r-nie:  L^T * x = y
			*/
			int i,j /* k */ ,dim;
			double sum;

			dim = L.Height;

			if (dim<1 || L.Width!=dim || b.Dimension!=dim)
				throw new Exception("Dimensions do not fit");

			Vector y = new Vector(dim); 

			for (i=0; i<dim; i++)
				if (L[i,i] == 0)
					throw new System.Exception("Wrong matrix");
			/*
			rozwiazanie r-nia: L * y = b
			*/
			for (i=0; i<dim; i++)
			{  
				sum = b[i];
				for (j=0; j<i; j++)
					sum -= L[i,j]*y[j];
				y[i] = sum/L[i,i];
			}
			/*
			rozwiazania r-nia: L^T*x=y
			w tym fragmencie programu logicznie wykonywane sa dzialania na L^T
			natomiast fizycznie wykonywane sa dzialania na L
			*/
			if (x==null)
				x = new Vector(dim); 
			else
				x.Dimension = dim; 
			for (i=dim-1; i>=0; i--)
			{  
				sum = y[i];
				for (j=i+1; j<dim; j++)
					sum -= L[j,i]*x[j];   // L_Number - odwrocone indexy
				x[i] = sum/L[i,i];
			}
		}

		public static void InverseL(Matrix L, ref Matrix invL)
		{
			int i,j,k,dim = L.Height;
			double sum; 

			if (dim<1 || dim!=L.Width)
				throw new Exception("Dimensions do not fit"); 

			for (i=0; i<dim; i++)
				if (L[i,i]==0) 
					throw new Exception("The matrix is not invertable");  

			Matrix.AssureDimensions(ref invL, dim,dim); 
			invL.FillWith(0);
	 
			for (j=0; j<dim; j++)
			{  
				invL[j,j] = 1.0/L[j,j];
				for (i=j+1; i<dim; i++)
				{
					sum=0;
					for (k=j; k<i; k++)
						sum += L[i,k]*invL[k,j];
					invL[i,j] = -sum/L[i,i];
				}
			}
		}

		public static void Inverse(Matrix A, ref Matrix invA)
		{
			int i,j,dim = A.Height;

			if (dim<1 || dim!=A.Width)
				throw new Exception("Dimensions do not fit"); 

			Matrix.AssureDimensions(ref invA, dim,dim); 

			Vector b = new Vector(dim), x = null;
			b.FillWith(0); 
	 
			for (j=0; j<dim; j++)
			{  
				b[j] = 1; 
				LinearEquationGaussElimination(A, ref x, b); 
				for (i=0; i<dim; i++)
					invA[i,j] = x[i]; 
				b[j] = 0; 
			}
		}

		public static bool AreEigenvaluesInside(Matrix D, double radius)
		{
			D *= radius; 
			for (int i=0; i<1000; i++)
			{
				D *= D; 
				if (D.NormSupremum>1e30) 
					return (false); 
				if (D.NormSupremum<1e-30)
					return (true); 
			}
			return (true); 
		}

		public static double MaxAbsoluteEigenvalue(Matrix D, double accuracy, int N)
		{
			int m = D.Height; 
			if (m!=D.Width)
				throw new Exception("Dimensions do not match"); 

			ASampler Sampler = new ASampler(); 
			Matrix DD = D*D; 

			Vector x = new Vector(m); 
			Sampler.FillWithNormals(ref x); 
			x /= Math.Sqrt(x*x); 
			double eigenval = Math.Abs(x*D*x);
			int n = 0;

			for (int i=0; true; i++)
			{
				Vector y = DD*x; 
				x = y/Math.Sqrt(y*y); 

				double eig = Math.Abs(x*D*x); 
				n = (Math.Abs(eig-eigenval)<accuracy) ? n+1 : 0; 
				eigenval = eig; 

				if (i>=m*2 && n>=N)
					return (eigenval); 
			}
		}

		public static double MaxAbsoluteEigenvalue2(Matrix D, double accuracy, int N)
		{
			int m = D.Height; 
			if (m!=D.Width)
				throw new Exception("Dimensions do not match"); 

			ASampler Sampler = new ASampler(); 

			Vector x = new Vector(m); 
			Sampler.FillWithNormals(ref x); 
			x /= Math.Sqrt(x*x); 
			double eigenval = Math.Abs(x*D*x);
			int n = 0;

			while (true)
			{
				Vector y = new Vector(m); 
				Sampler.FillWithNormals(ref y); 
				y = y - x*(x*y); 
				y /= Math.Sqrt(y*y);

				double xy = x*y; 
				double yy = y*y;

				double xDx = x*D*x;
				double xDy = x*D*y; 
				double yDx = y*D*x; 
				double yDy = y*D*y; 

				double M = Math.Sqrt(Sq(xDy + yDx) + Sq(xDx - yDy)); 
				double sin_tau = (xDy+yDx)/M; 
				double cos_tau = (xDx-yDy)/M;
				double sin_arc1 = 0; 
				double cos_arc1 = 1; 
				if (cos_tau>0)
				{
					cos_arc1 = Math.Sqrt(0.5*(1.0+cos_tau));
					sin_arc1 = sin_tau/2/cos_arc1;
				}
				else
				{
					sin_arc1 = Math.Sqrt(0.5*(1.0-cos_tau));
					cos_arc1 = sin_tau/2/sin_arc1; 
				}

				double cos_arc2 = -sin_arc1;
				double sin_arc2 = cos_arc1; 
				Vector x1 = x*cos_arc1 + y*sin_arc1; 
				Vector x2 = x*cos_arc2 + y*sin_arc2; 
				y =(Math.Abs(x1*D*x1)>Math.Abs(x2*D*x2)) ? x1 : x2; 
				double dlug = Math.Sqrt(y*y); 
				y /= dlug; 
				double eig = Math.Abs(y*D*y);
				n = (eig-eigenval<accuracy) ? n+1 : 0; 
				x = y; 
				eigenval = eig; 
				if (n>=N)
				{
					Vector t1 = D*x;
					return eigenval;
				}
			}
		}
		#endregion

		#region Operations with matrix and vector
		public static void Copy(Matrix pattern, ref Vector v)
		{
			int dim = Math.Max(pattern.Height, pattern.Width); 
			Vector.AssureDimension(ref v, dim); 
			if (pattern.Width==1)
			{
				for (int i=0; i<dim; i++)
					v[i] = pattern[i,0];
			}
			else
				if (pattern.Height==1)
			{
				for (int j=0; j<dim; j++)
					v[j] = pattern[0,j];
			}
			else
				throw new Exception("The matrix is not a vector"); 
		}

		public static void Copy(Vector pattern, bool column_vector, ref Matrix m)
		{
			int dim = pattern.Dimension; 
			if (column_vector)
			{
				Matrix.AssureDimensions(ref m, dim, 1); 
				for (int i=0; i<dim; i++)
					m[i,0] = pattern[i];
			}
			else
			{
				Matrix.AssureDimensions(ref m, 1, dim); 
				for (int j=0; j<dim; j++)
					m[0,j] = pattern[j];
			}
		}

		public static void AddSquaredVector(ref Matrix m, Vector a)
		{
			int dim = a.Dimension; 
			if (dim!=m.Height || dim!=m.Width)
				throw (new System.Exception("Adding squared vector is impossible due to wrong dimensions")); 
			for (int i=0; i<dim; i++)
				for (int j=0; j<dim; j++)
					m[i,j] += a[i]*a[j]; 
		}

        public static void UpdateInvSumOfSquares(ref Matrix M, double weight_invM, Vector x, double weight_x)
        {
            if (M.Height != x.Dimension || M.Width != x.Dimension)
                throw new Exception("Dimensions do not match");

            x *= Math.Sqrt(weight_x);

            Vector xT_M = x * M;
            Matrix MT_x_xT_M = xT_M | xT_M;
            double xT_M_x = xT_M * x;
            double denominator = xT_M_x + weight_invM;

            MT_x_xT_M /= denominator;

            M -= MT_x_xT_M;
            M /= weight_invM;
        }

        public static void InsertMatrix2Vector(ref Vector v, int v_position, Matrix m)
        {
            if (v_position + m.Height * m.Width > v.Dimension)
                throw new Exception("Computing.MatrixToolbox.InsertMatrix2Vector()");
            for (int i = 0; i < m.Height; i++)
                for (int j = 0; j < m.Width; j++)
                    v[v_position++] = m[i, j];
        }

        public static void AddScaledSubVector2Matrix(ref Matrix m, Vector v, int v_position, double scalar)
        {
            if (v.Dimension - v_position < m.Height * m.Width)
                throw new Exception("Computing.MatrixToolbox.AddScaledSubVector2Matrix()");
            for (int i = 0; i < m.Height; i++)
                for (int j = 0; j < m.Width; j++)
                    m[i, j] += v[v_position++] * scalar; 
        }
        #endregion
    }
}
