using Controller;
using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace WpfVersion
{
    public class DataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string TrackName => $"Track: {Data.CurrentRace?.Track.Name}";
        public List<string> Equipment => Data.Competition.equipment.Select(x => $"{x.Key.Name} - Speed: {x.Value}").ToList();
        public List<string> Points => Data.Competition.points.Select(x => $"{x.Key.Name} - Points: {x.Value}").ToList();
        public string NextTrack => Data.Competition.Tracks.Count != 0 ? Data.Competition.Tracks.Peek().Name : "N/A";
        public string StartTime => Data.CurrentRace?.StartTime.ToString("dddd, dd MMMM HH:mm:ss");
        public List<string> TimesBrokenDown => Data.Competition.timesBrokenDown.Select(x => $"{x.Key.Name} - broken down: {x.Value}x").ToList();


        public DataContext()
        {
            if (Data.CurrentRace != null)
            {
                Data.CurrentRace.DriversChanged += OnDriversChanged;
            }
        }

        public void OnDriversChanged(object s, DriversChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }
    }
}