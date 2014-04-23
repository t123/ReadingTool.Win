using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using RTWin.Annotations;

namespace RTWin.Models.Dto
{
    public class BaseDtoModel : INotifyPropertyChanged
    {
        protected Dictionary<string, bool> _errors = new Dictionary<string, bool>();
        
        public bool IsValid
        {
            get { return !_errors.Any(x => x.Value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}