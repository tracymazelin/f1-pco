using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace F1toPCO.Model {
    public class BaseModel : INotifyPropertyChanged {
                
        [XmlIgnore]
        public bool IsDirty { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void SetField<T>(ref T field, T value, string propertyName) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                IsDirty = true;
                OnPropertyChanged(propertyName);
            }
        }
        protected virtual void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}