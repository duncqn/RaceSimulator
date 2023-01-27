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
        public string TrackName => $"Track: {Data.CurrentRace.Track.Name}";
        
        //Value for times broken down during a competition, a list is created for every participant.
        public List<string> TimesBrokenDown => Data.Competition.TimesBrokenDown.Select(participant => $"{participant.Key.Name} is {participant.Value}x kapot gegaan.").ToList();
        
        //Value for completed laps during a single race, a list is created for every participant.
        public List<string> CompletedLaps => Data.CurrentRace.LapsCompleted.Select(participant => $"{participant.Key.Name} heeft {participant.Value} laps gereden.").ToList();
        
        /// <summary>
        /// Value for the speed that a participant is driving during a race, a list is created for every participant.
        /// </summary>
        public List<string> ParticipantSpeed => Data.Competition.Speed.Select(participant => $"{participant.Key.Name} gaat {participant.Value} kilometer per uur.").ToList();
        
        //Value for the time a race is started.
        public string StartTime => $"Starttijd: {Data.CurrentRace.StartTime:dddd, dd MMMM HH:mm:ss}";
        
        //List that is filled with every track that is in the competition.
        public List<string> NextTrack => Data.Competition.Tracklist.Select(x => $"{x}").ToList();

        public void OnDriversChanged(object s, DriversChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }
    }
}