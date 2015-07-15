using Inzynierka.Model.ControlAlgorithm;
using Inzynierka.Model.ControlAlgorithm.PredictionControl;
using Inzynierka.Model.Model;
using System;
using System.Threading;
using System.Windows.Input;

namespace Inzynierka.ViewModel
{
    public class SimulationViewModel : ViewModelBase
    {
        public double Value { get; set; }

        public IModel Model { get; set; }

        public IAlgorithm Algorithm { get; set; }

        public Boolean Loop { get; set; }

        public Boolean Faster { get; set; }

        public Boolean IsFinished { get; set; }

        public string TEXT { get; set; }


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
                    Value = Algorithm.GetValueTMP();
                    OnPropertyChanged("Value");
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

            #region Thread

            Value = 0;
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
                    Value = Algorithm.GetValueTMP();
                    OnPropertyChanged("Value");
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
                    Value = Algorithm.GetValueTMP();
                    OnPropertyChanged("Value");
                }
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}