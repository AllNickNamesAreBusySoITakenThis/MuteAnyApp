using MuteAnyApp.Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MuteAnyApp.Core.Types
{
    [JsonObject]
    public class SoundChangeAction : INotifyPropertyChanged
    {
        private string processName;
        private string name;
        private ActionType actionType;
        private float setToValue;

        public SoundChangeAction()
        {
        }

        public Guid Id { get; set; }

        /// <summary>
        /// Описание действия
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged(nameof(Name)); }
        }

        /// <summary>
        /// Тип действия
        /// </summary>      
        public ActionType ActionType
        {
            get { return actionType; }
            set { actionType = value; OnPropertyChanged(nameof(ActionType)); }
        }

        /// <summary>
        /// Имя процесса
        /// </summary>
        public string ProcessName
        {
            get { return processName; }
            set { processName = value; OnPropertyChanged(nameof(ProcessName)); }
        }


        /// <summary>
        /// Значение звука, которое будет установлено (для типа SetTo)
        /// </summary>       
        public float SetToValue
        {
            get { return setToValue; }
            set { setToValue = value; OnPropertyChanged(nameof(SetToValue)); }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
