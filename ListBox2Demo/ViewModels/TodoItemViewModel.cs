using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;//add referance
using System.Collections.ObjectModel;////add referance
using System.ComponentModel;


namespace ListBox2Demo.ViewModels
{
    public class Thing : INotifyPropertyChanged
    {
        #region Completed
        [XmlElement("Completed")]
        private bool isCompleted = false;
        public bool IsCompleted
        {
            get
            {
                return isCompleted;
            }
            set
            {
                isCompleted = value;
                OnPropertyChanged("IsCompleted");
            }
        }
        #endregion

        #region HasAlarm
        [XmlElement("HasAlarm")]
        private bool hasAlarm = false;
        public bool HasAlarm
        {
            get
            {
                return hasAlarm;
            }
            set
            {
                hasAlarm = value;
                OnPropertyChanged("HasAlarm");
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class Todo : INotifyPropertyChanged
    {
        public IList<Thing> Options { get; set; }

        #region Name
        [XmlElement("Name")]
        public string name = null;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        #endregion

        #region HasAlarm
        [XmlElement("HasAlarm")]
        private bool hasAlarm = false;
        public bool HasAlarm
        {
            get
            {
                return hasAlarm;
            }
            set
            {
                hasAlarm = value;
                OnPropertyChanged("HasAlarm");
            }
        }
        #endregion

        #region AlarmTime
        [XmlElement("AlarmTime")]
        public DateTime AlarmTime { get; set; }
        #endregion

        #region Completed
        [XmlElement("Completed")]
        private bool isCompleted = false;
        public bool IsCompleted
        {
            get
            {
                return isCompleted;
            }
            set
            {
                isCompleted = value;
                OnPropertyChanged("IsCompleted");
            }
        }
        #endregion

        #region Importance
        [XmlElement("Importance")]
        private bool importance = false;
        public bool Importance
        {
            get
            {
                return importance;
            }
            set
            {
                importance = value;
                OnPropertyChanged("Importance");
            }
        }
        #endregion

        #region Expande
        private bool isExpanded;
        public bool IsExpanded
        {
            get
            {
                return this.isExpanded;
            }
            set
            {
                if (this.isExpanded != value)
                {
                    this.isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }
            }
        }

        public bool IsItemExpanded { get; set; }
        #endregion

        #region ReminderName
        private string reminderName = null;
        public string ReminderName
        {
            get
            {
                return reminderName;
            }
            set
            {
                reminderName = value;
            }
        }
        #endregion

        #region HasNoOptions
        public bool HasNoOptions
        {
            get
            {
                return this.Options == null || this.Options.Count == 0;
            }
        }    
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
