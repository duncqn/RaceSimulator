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
        public List<string> TimesBrokenDown => Data.Competition?.timesBrokenDown?.Select(x => $"{x.Key.Name} is {x.Value}x kapot gegaan.").ToList();
        public List<string> Lap => Data.CurrentRace?._lapsCompleted?.Select(i => $"{i.Key.Name} heeft {i.Value} laps gereden.").ToList();
        public List<string> Equipment => Data.Competition.speed.Select(x => $"{x.Key.Name} gaat {x.Value} kilometer per uur.").ToList();
        public string StartTime => $"Starttijd: {Data.CurrentRace?.StartTime.ToString("dddd, dd MMMM HH:mm:ss")}";
        public List<string> NextTrack => Data.Competition.tracklist.Select(x => $"{x}").ToList();

        public void OnDriversChanged(object s, DriversChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }
    }
}