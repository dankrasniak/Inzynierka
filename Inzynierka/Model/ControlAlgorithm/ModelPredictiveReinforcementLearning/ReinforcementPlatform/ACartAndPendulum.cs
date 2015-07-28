using System;

namespace Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcementLearning.ReinforcementPlatform
{
	public class ACartAndPendulum
	{
		#region Fields
		//	parameters
		protected double	g;		//	przyspieszenie grawitacyjne
		protected double	m_c;	//	masa wozka
		protected double	m_p;	//	masa preta
		protected double	l;		//	polowa dlugosci preta
		protected double	mi_c;	//	wspolczynnik tarcia wozka na torze
		protected double	mi_p;	//	wspolczynnik tarcia preta na wozku
		protected double	ExternalDiscretization;
		protected double	InternalDiscretization; 

		//	state
		private double	X; 
		private double	dX; 
		private double	Theta; 
		private double	SinTheta; 
		private double	CosTheta; 
		private double	dTheta; 
		#endregion

		#region Auxiliary functions
		static protected double	sqr(double x)	{	return (x*x);	}
		static protected double	sgn(double x)	{	return (x<0 ? -1 : 1);	}
		#endregion

		#region Construction
		public ACartAndPendulum()
		{
			SetDefaultParameters(); 

			Position = Velocity = 0; 
			Arc = ArcVelocity = 0; 
		}
		#endregion

		#region Getting and setting state
		protected void RectifyState()
		{
			while (Theta<0)
				Theta += Math.PI*2; 
			while (Theta>Math.PI*2)
				Theta -= Math.PI*2; 
			SinTheta = Math.Sin(Theta); 
			CosTheta = Math.Cos(Theta); 
		}

		public double Position
		{
			set { X = value; } 
			get { return X;  } 
		}

		public double Velocity
		{
			set { dX = value; } 
			get { return dX;  } 
		}

		public double Arc
		{
			set { Theta = value;	RectifyState(); }
			get { return (Theta); }
		}

		public double SinArc
		{
			get { return (SinTheta); }
		}

		public double CosArc
		{
			get { return (CosTheta); } 
		}

		public double ArcVelocity
		{
			set { dTheta = value; } 
			get { return dTheta; } 
		}
		#endregion

		#region Geting and Seting parameters
		public void SetDefaultParameters()
		{
			SetParameters0(9.81, 1, 0.1, 0.5, 0.0005, 0.000002); 
			SetParameters1(0.02, 0.002); 
		}

		public void	SetParameters0(
			double	_g,		//	przyspieszenie grawitacyjne
			double	_m_c,	//	masa wozka
			double	_m_p,	//	masa preta
			double	_l,		//	polowa dlugosci preta
			double	_mi_c,	//	wspolczynnik tarcia wozka na torze
			double	_mi_p	//	wspolczynnik tarcia preta na wozku
			)
		{
			if (_g>0)		g = _g; 
			if (_m_c>0)		m_c = _m_c;
			if (_m_p>0)		m_p = _m_p;
			if (_l>0)		l = _l;
			if (_mi_c>=0)	mi_c = _mi_c;
			if (_mi_p>=0)	mi_p = _mi_p;
		}

		public void SetParameters1(
			double external_discretization, 
			double internal_discretization
			)
		{
			ExternalDiscretization = external_discretization; 
			InternalDiscretization = internal_discretization; 
		}

		public void ConfuseParameters(double p)
		{
			System.Random sampler = new System.Random(); 
			g   += g   *p*(sampler.NextDouble()*2-1); 
			m_c += m_c *p*(sampler.NextDouble()*2-1); 
			m_p += m_p *p*(sampler.NextDouble()*2-1); 
			l   += l   *p*(sampler.NextDouble()*2-1); 
			mi_c+= mi_c*p*(sampler.NextDouble()*2-1); 
			mi_p+= mi_p*p*(sampler.NextDouble()*2-1); 
		}
		#endregion

		#region Derivatives
		public void TimeDerivatives(double dx, double sin_theta, double cos_theta, double dtheta, double F, ref double d2x_dt, ref double d2theta_dt)
		{
			double insided2Theta = (-F - m_p*l*sqr(dtheta)*sin_theta + mi_c*sgn(dx)) / (m_c+m_p); 
			d2theta_dt =
				( g*sin_theta + cos_theta*insided2Theta - mi_p*dtheta/(m_p*l) )
				/ 
				( l*(4.0/3.0 - m_p*sqr(cos_theta)/(m_c+m_p)) );

			double insided2X = ( sqr(dtheta)*sin_theta - d2theta_dt*cos_theta ); 
			d2x_dt = 
				( F + m_p*l*insided2X - mi_c*sgn(dx) ) 
				/ 
				(m_c + m_p); 
		}

		public void ForceDerivatives(double cos_theta, ref double d2x_dF, ref double d2theta_dF)
		{
			d2theta_dF = - cos_theta
				/ (l * ((m_c+m_p)*4/3 - m_p*sqr(cos_theta))); 

			d2x_dF = (1.0 - m_p*l*d2theta_dF*cos_theta)
				/ (m_c+m_p); 
		}
		#endregion

		#region Moving a step
		public void MoveAStep(double force)
		{
			for (double t=0; t<ExternalDiscretization; t+=InternalDiscretization)
			{
				double dt = Math.Min(InternalDiscretization, ExternalDiscretization-t); 
				double dt2 = dt*dt; 

				//	start
				double d2x=0, d2theta=0; 
				this.TimeDerivatives(
					dX, SinTheta, CosTheta, dTheta, force, 
					ref d2x, ref d2theta
					); 

				X += dX*dt + 0.5*d2x*dt2; 
				dX+= d2x*dt; 

				Theta += dTheta*dt + 0.5*d2theta*dt2; 
				dTheta+= d2theta*dt; 

				RectifyState(); 
			}
		}
		#endregion
	}
}
