using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ListBox2Demo.ViewModels
{
    public class TodoDataViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Todo> todos_Today;

        private void InitializeItems()
        {
            this.todos_Today = (App.Current as App).todos_Today;
            Todo todo = new Todo();
            todo.Name = "Good";
            todo.Options = new List<Thing>()
            {
                new Thing()
            };

            this.todos_Today.Add(todo);
        }

        /// <summary>
        /// A collection for <see cref="DataItemViewModel"/> objects.
        /// </summary>
        public ObservableCollection<Todo> Items
        {
            get
            {
                if (this.todos_Today == null)
                {
                    this.InitializeItems();
                }
                return this.todos_Today;
            }
            private set
            {
                this.todos_Today = value;
                OnPropertyChanged("Items");
            }
        }

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
