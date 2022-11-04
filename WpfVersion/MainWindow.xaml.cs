using System;
using System.Windows;
using System.Windows.Threading;
using Controller;
using Model;

namespace WpfVersion
{
    public partial class MainWindow : Window
    {

        private StatRace sr = new StatRace();
        private StatPar sp = new StatPar();

        public DataContext dataContext = new DataContext();

        public MainWindow()
        {
            Initialize();
            InitializeComponent();
            DataContext = dataContext;
            sp.DataContext = dataContext;
            sr.DataContext = dataContext;
        }

        private void Initialize()
        {
            Data.Initialize();
            Data.NextRace();
            VisualisationWpf.Initialize();
            Events();
            Data.CurrentRace.Start();
        }

        private void InitializeNextRace(object? sender, EventArgs args)
        {
            Cache.ClearCache();
            Data.CurrentRace.CleanUp();
            Data.NextRace();
            VisualisationWpf.Initialize();
            Events();
            Data.CurrentRace.Start();
        }

        private void Events()
        {
            Data.CurrentRace.DriversChanged += DriversChanged;
            Data.CurrentRace.RaceFinished += InitializeNextRace;
            Data.CurrentRace.DriversChanged += dataContext.OnDriversChanged;
        }

        private void DriversChanged(object? sender, DriversChangedEventArgs e)
        {
            Tekening.Dispatcher.BeginInvoke(
                DispatcherPriority.Render,
                new Action(() =>
                {
                    Tekening.Source = null;
                    Tekening.Source = VisualisationWpf.DrawTrack(e.Track, (int)Width, (int)Height);
                })
            );
        }


        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
        }

        private void MenuItem_StatRace_Click(object sender, RoutedEventArgs e)
        {
            sr.Show();
        }

        private void MenuItem_StatPar_Click(object sender, RoutedEventArgs e)
        {
            sp.Show();
        }
    }
}