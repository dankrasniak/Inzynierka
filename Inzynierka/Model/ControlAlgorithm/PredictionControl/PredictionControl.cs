﻿using Inzynierka.Model.Logger;
using Inzynierka.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inzynierka.Model.ControlAlgorithm.PredictionControl
{
    public class PredictionControl : IAlgorithm
    {
        private readonly LogIt _logger;

        private readonly IModel _model;
        private List<Double> _state;
        private List<List<Double>> _horizon;
        private readonly int _horizonSize;
        private int _iterationNr;
        private readonly int _iterationsLimit;    // max czasu na jeden epizod
        private readonly double[] _minAction;    // min sterowania 
        private readonly double[] _maxAction;    // max sterowania

        public PredictionControl(IModel model, List<Property> properties, List<LoggedValue> loggedValues) 
        {
            _logger = new LogIt("", loggedValues); // TODO Add Path

            // Variables from properties list
            M = (int) Convert.ToDouble(properties.Find(p => p.Name.Equals("M")).Value);
            _startSigma = Convert.ToDouble(properties.Find(p => p.Name.Equals("StartSigma")).Value);
            _horizonSize = (int)Convert.ToDouble(properties.Find(p => p.Name.Equals("Horizon")).Value);
            _sigmaMin = Convert.ToDouble(properties.Find(p => p.Name.Equals("SigmaMin")).Value);
            var externalDiscretization =
                Convert.ToDouble(properties.Find(p => p.Name.Equals("ExternalDiscretization")).Value);
            var internalDiscretization =
                Convert.ToDouble(properties.Find(p => p.Name.Equals("InternalDiscretization")).Value);
            var TimeLimit = (int)Convert.ToDouble(properties.Find(p => p.Name.Equals("TimeLimit")).Value);

            _iterationsLimit = (int)(TimeLimit / externalDiscretization);

            _model = model;
            _model.SetDiscretizations(externalDiscretization, internalDiscretization);
            _state = new List<double>(_model.GetInitialState());
            _minAction = _model.MinActionValues();
            _maxAction = _model.MaxActionValues();

            GenerateHorizon();
            _iterationNr = 0;
        }

        private void GenerateHorizon()
        {
            _horizon = new List<List<Double>>();

            for (int i = 0; i < _horizonSize; ++i)
            {
                _horizon.Add(_model.GenerateControlVariables());
            }
        }

        public List<Double> GetValueTMP()
        {
            if (_iterationNr++ > _iterationsLimit)
                NextEpisode();

            var controlVariables = GetControlVariables();
            _logger.Log("Wartość wejściowa", 
                controlVariables.Aggregate("", (current, t) => current + (FormatDouble(t) + " ")));

            _state = _model.StateFunction(_state, controlVariables);

            _logger.Log("stateVariables", 
                _state.Aggregate("", (s, d) => s + FormatDouble(d) + " "));

            if (!_model.IsStateAcceptable(_state))
                NextEpisode();

            var result = _model.GetValue(_state);
            var temp = result.Aggregate("", (current, add) => current + add);
            _logger.Log("Wartość wyjściowa", temp);

            return result;
        }

        private void NextEpisode()
        {
            _state = new List<double>(_model.MeddleWithGoalAndStartingState());
            GenerateHorizon();
            _iterationNr = 0;
        }

        private List<Double> GetControlVariables()
        {
            PrepareHorizont();

            _logger.Log("horizon", 
                _horizon.Aggregate("", (s, list) => s + FormatDouble(list[0]) + " "));

            return _horizon[0];
        }

        #region EvoAlg

        private readonly int M = 10;
        private const double C1 = 0.82;
        private const double C2 = 1.2;
        private readonly double _sigmaMin;

        private double _sigma;                  // Standard deviation
        private readonly double _startSigma;
        private int _phi;                       // Number of times the child specimen was chosen over the parent specimen in the last cicle of M iterations
        private int _iterationNum;

        private void PrepareHorizont()
        {
            UpdateHorizon();

            _phi = 0;
            _iterationNum = 0;
            _sigma = _startSigma;

            while (!IsFinished())
            {
                ++_iterationNum;
                NextIteration();
            }
        }

        private void NextIteration()
        {
            var modifiedHorizon = ModifyHorizon(_horizon);

            var currentHorizonValue = GetHorizonValue(_horizon);
            var newHorizonValue = GetHorizonValue(modifiedHorizon);

            _logger.Log("Possible states", "Possible states: " + FormatDouble(currentHorizonValue) + " VS: " + FormatDouble(newHorizonValue)); // TODO

            if (currentHorizonValue < newHorizonValue)
            {
                _horizon = modifiedHorizon;
                ++_phi;
            }
            UpdateSigma();
        }

        private void NextIteration2()
        {

            var modifiedHorizon = ModifyHorizon(_horizon);

            var currentHorizonState = GetFinalState(_state, _horizon);
            var newHorizonState = GetFinalState(_state, modifiedHorizon);

            var currentHorizonValue = _model.GetDiscrepancy(currentHorizonState);
            var newHorizonValue = _model.GetDiscrepancy(newHorizonState);

            _logger.Log("Possible states", "Possible states: " + FormatDouble(currentHorizonValue) + " VS: " + FormatDouble(newHorizonValue));

            if (!_model.IsFirstBetter(currentHorizonState, newHorizonState))
            {
                _horizon = modifiedHorizon;
                ++_phi;
            }
            UpdateSigma();
        }

        private double GetHorizonValue(IEnumerable<List<double>> horizon)
        {
            var horizonValue = 0.0;
            var horizonStatesList = GetHorizonStatesList(_state, horizon);

            for (int i = 0; i < _horizonSize; ++i)
            {
                if (_model.IsStateAcceptable(horizonStatesList[i]))
                    horizonValue += (i + 1)*_model.GetReward(horizonStatesList[i]);
                else
                {
                    for (; i < _horizonSize; ++i)
                    {
                        horizonValue += ModelPenalty();
                    }
                }
                //horizonValue -= (i+1)*Math.Pow(_model.GetDiscrepancy(horizonStatesList[i]), 2);
            }

            return horizonValue;
        }

        private double ModelPenalty()
        {
            return -25000.0;//_model.Penalty(); // TODO
        }

        private List<List<Double>> ModifyHorizon(List<List<Double>> horizon)
        {
            var modifiedHorizon = new List<List<Double>>();
            var CONTROL_VARIABLES_NR = (horizon[0]).Count;

            for (int i = 0; i < _horizonSize; ++i)
            {
                modifiedHorizon.Add(new List<Double>(horizon[i]));

                for (int j = 0; j < CONTROL_VARIABLES_NR; ++j)
                {
                    modifiedHorizon[i][j] = Math.Min(
                        Math.Max(_minAction[j], horizon[i][j] + _sigma * GetGaussian()),
                        _maxAction[j]);
                    //Math.Abs(horizon[i][j] + _sigma * GetGaussian());
                }
            }

            return modifiedHorizon;
        }

        private bool IsFinished()
        {
            return _sigma < _sigmaMin;
        }

        private void UpdateSigma()
        {
            if (_iterationNum % M != 0)
                return;

            if ((double)_phi / M < 0.2)
            {
                _sigma *= C1;
            }
            else if ((double)_phi / M > 0.2)
            {
                _sigma *= C2;
            }
            _phi = 0; // == 0.2
        }

        private readonly Random _rand = new Random();

        private double GetGaussian() // TODO Verify
        {
//            double u1 = _rand.NextDouble(); //these are uniform(0,1) random doubles
//            double u2 = _rand.NextDouble();
//            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
//                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
//            //double randNormal =
//            //             mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
//            return randStdNormal;

            double z = -Math.Log(1.0 - _rand.NextDouble());
            double alpha = _rand.NextDouble() * Math.PI * 2;
            double norm = Math.Sqrt(z * 2) * Math.Cos(alpha);
            return norm * 1.0;
        }

        #endregion EvoAlg

        private List<Double> GetFinalState(List<double> initialState, IEnumerable<List<double>> horizon)
        {
            throw new NotSupportedException();
            //return horizon.Aggregate(initialState, RungeKuttha);
        }

        private List<List<Double>> GetHorizonStatesList(IEnumerable<double> initialState, IEnumerable<List<double>> horizon)
        {
            var state = new List<Double>(initialState);
            var stateList = new List<List<Double>>();
            foreach (var controlValues in horizon)
            {
                state = _model.StateFunction(state, controlValues);

                stateList.Add(new List<Double>(state));
            }
            return stateList;
        }

        private void UpdateHorizon()
        {
            _horizon.RemoveAt(0);
            _horizon.Add(_horizon[_horizonSize - 2]);
        }

        private void UpdateHorizon2()
        {
            _horizon.RemoveAt(0);
            _horizon.Add(_model.GenerateControlVariables());
        }

        private static string FormatDouble(Double value)
        {
            return value.ToString(String.Format("{0}{1}", "F", 4));
        }
    }
}