using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Threading;
using System.Windows.Input;
using Inzynierka.Model.ControlAlgorithm.PredictionControl;
using Inzynierka.Model.Model;
using Inzynierka.View;
using Xceed.Wpf.Toolkit.Panels;

namespace Inzynierka.ViewModel
{
    public class SimulationViewModel : ViewModelBase
    {
        public double Value { get; set; }

        public IModel Model { get; set; }

        public IAlgorithm Algorithm { get; set; }

        public Boolean Loop { get; set; }

        public string TEXT { get; set; }


        #region Buttons

        public ICommand StepButton { get; set; }
        public ICommand StopButton { get; set; }
        public ICommand PlayButton { get; set; }

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
                {
                    // Step of a simulation

                    Value = ((PredictionControl) Algorithm).GetValueTMP();
                    OnPropertyChanged("Value");
                },
                () =>
                {
                    // Not playing?
                    return true;
                }
            );
            StopButton = new ButtonCommand(
                () =>
                {
                    // Stops the simulation
                    Loop = false;
                },
                () =>
                {
                    // Simulation must be played
                    return true;
                }
            );
            PlayButton = new ButtonCommand(
                () =>
                {
                    // Starts the simulation
                    // New thread, receives events => stop
                    Loop = true;
                },
                () =>
                {
                    // Must not be played?
                    return true;
                }
            );
            #endregion ButtonInitialisation

            #region Thread

            Value = 0;
            Loop = false;
            var thread = new Thread(this.Foo);
            thread.Start();
            #endregion Thread
        }

        public void Foo()
        {
            while (true)
            {
                while (Loop)
                {
                    Value = ((PredictionControl)Algorithm).GetValueTMP();
                    OnPropertyChanged("Value");
                }
                Thread.Sleep(1000);
            }
        }
    }
}