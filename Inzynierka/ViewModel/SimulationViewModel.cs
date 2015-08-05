using System.Windows.Controls;
using Inzynierka.Model;
using Inzynierka.Model.ControlAlgorithm;
using Inzynierka.Model.Model;
using Inzynierka.Model.Model.Pendulum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Inzynierka.View;
using Inzynierka.ViewModel.Visualisations;

namespace Inzynierka.ViewModel
{
    public class SimulationViewModel : ViewModelBase
    {

        private List<Double> _value;
        public object Value // TODO Pendulum
        {
            get { return _value.Aggregate("", (current, added) => current + added.ToString() + "\n"); }
            set
            {
                _value = (List<Double>) value;
                if (_value.Count != 0)
                {
                    _visualisator.SetValue(_value);
//                    if (Model is Pendulum)
//                    {
//                        PendulumX = _value[0]*200/((Pendulum) Model)._XMAX + 325.0;
//                        PendulumT = _value[1]*180/Math.PI;
//                    }
//                    else
//                        PendulumT = _value[0]*90/25000.5 - 90;
                }
//                OnPropertyChanged("PendulumX");
//                OnPropertyChanged("PendulumT");
            }
        }

//        public double PendulumX { get; set; }

//        public double PendulumT { get; set; }

        public IModel Model { get; set; }

        private IVisualisator _visualisator;

        private UserControl _content;

        public UserControl Content
        {
            get { return _content; }
            set
            {
                _content = value;
                _visualisator = (IVisualisator) _content.DataContext;
                OnPropertyChanged("Content");
            }
        }

        #region Properties
        private List<Property> _propertiesA;

        public List<Property> PropertiesA
        {
            get
            {
                return _propertiesA;
            }
            set
            {
                _propertiesA = value;
                OnPropertyChanged("PropertiesA");
            }
        }

        private List<Property> _propertiesM;

        public List<Property> PropertiesM
        {
            get
            {
                return _propertiesM;
            }
            set
            {
                _propertiesM = value;
                OnPropertyChanged("PropertiesM");
            }
        }
        #endregion Properties

        public IAlgorithm Algorithm { get; set; }

        public Boolean Loop { get; set; }

        public Boolean Faster { get; set; }

        public Boolean IsFinished { get; set; }

        public int TimeIndex { get; set; }

        public int EpisodeNumber { get; set; }


        #region Buttons

        public ICommand StepButton { get; set; }
        public ICommand StopButton { get; set; }
        public ICommand PlayButton { get; set; }
        public ICommand FasterButton { get; set; }


        #endregion Buttons

        public SimulationViewModel()
        {
            /** Symulacja.
             * Wykorzystując zdarzenia:
             * wysyła zdarzenia, np. play, stop, step
             * odbiera zdarzenia, np. wartości
             * 
             * Wykorzystując pętlę:
             * co pętlę sprawdza wartości, i je ustawia.
             * 
             */

            #region ButtonInitialisation
            StepButton = new ButtonCommand(
                () =>
                { // Step of a simulation
                    var data = Algorithm.GetValueTMP();
                    Value = data.Values;
                    OnPropertyChanged("Value");
                    TimeIndex = data.IterationNumber;
                    OnPropertyChanged("TimeIndex");
                    EpisodeNumber = data.EpisodeNumber;
                    OnPropertyChanged("EpisodeNumber");
                },
                () =>
                { // Not playing?
                    return !Loop && !Faster;
                }
            );
            StopButton = new ButtonCommand(
                () =>
                { // Stops the simulation
                    Loop = false;
                    Faster = false;
                },
                () =>
                { // Simulation must be played
                    return true;
                }
            );
            PlayButton = new ButtonCommand(
                () =>
                { // Starts the simulation. New thread, receives events => stop
                    Loop = true;
                    Faster = false;
                },
                () =>
                { // Must not be played?
                    return true;
                }
            );
            FasterButton = new ButtonCommand(
                () =>
                {
                    Loop = false;
                    Faster = true;
                },
                () =>
                {
                    return true;
                }
            );
            #endregion ButtonInitialisation

            //PendulumX = 325.0;

            #region Thread

            //Content = new PendulumV();
            _value = new List<double>();
            Loop = false;
            IsFinished = false;
            var thread = new Thread(this.Foo);
            thread.Start();

            #endregion Thread
        }

        private void Foo()
        {
            while (!IsFinished)
            {
                while (Loop)
                {
                    var data = Algorithm.GetValueTMP();
                    Value = data.Values;
                    OnPropertyChanged("Value");
                    TimeIndex = data.IterationNumber;
                    OnPropertyChanged("TimeIndex");
                    EpisodeNumber = data.EpisodeNumber;
                    OnPropertyChanged("EpisodeNumber");
                }
                while (Faster)
                {
                    Algorithm.GetValueTMP();
                    Algorithm.GetValueTMP();
                    Algorithm.GetValueTMP();
                    Algorithm.GetValueTMP();
                    Algorithm.GetValueTMP();
                    Algorithm.GetValueTMP();
                    Algorithm.GetValueTMP();
                    Algorithm.GetValueTMP();
                    Algorithm.GetValueTMP();
                    var data = Algorithm.GetValueTMP();
                    Value = data.Values;
                    OnPropertyChanged("Value");
                    TimeIndex = data.IterationNumber;
                    OnPropertyChanged("TimeIndex");
                    EpisodeNumber = data.EpisodeNumber;
                    OnPropertyChanged("EpisodeNumber");
                }
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}