using RateController.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RateController.ViewModels
{
    public class RelaysViewModel : ViewModelBase
    {
       
        public ObservableCollection<Relay> Relays { get; }

        public RelaysViewModel()
        {
        
          
            List<Relay> relays = new List<Relay>();
            relays.Add(new Relay() { Id = 1, Type = "John Doe", SectionNumber = 1 });
            relays.Add(new Relay() { Id = 2, Type = "Jane Doe", SectionNumber = 2 });
            relays.Add(new Relay() { Id = 3, Type = "Sammy Doe", SectionNumber = 3 });
            
            Relays = new ObservableCollection<Relay>(relays);
            
          
        }
    }
}



