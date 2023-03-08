using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuteAnyApp.Core.Types
{
    [JsonObject]
    public class SoundChangeScenario:INotifyPropertyChanged
    {
        private Guid id;
        private string name;
        private List<SoundChangeAction> actions = new List<SoundChangeAction>();

        public SoundChangeScenario()
        {

        }

        public Guid Id
        {
            get { return id; }
            set { id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged(nameof(Name)); }
        }

        public List<SoundChangeAction> Actions
        {
            get { return actions; }
            set { actions = value; OnPropertyChanged(nameof(Actions)); }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
