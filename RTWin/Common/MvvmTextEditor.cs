using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ICSharpCode.AvalonEdit;

namespace RTWin.Common
{
    /// <summary>
    /// <see cref="http://stackoverflow.com/a/22967552/215538"/>
    /// </summary>
    public class MvvmTextEditor : TextEditor, INotifyPropertyChanged
    {
        public static DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof (string), typeof (MvvmTextEditor),
                // binding changed callback: set value of underlying property
                new PropertyMetadata((obj, args) =>
                {
                    MvvmTextEditor target = (MvvmTextEditor) obj;
                    if (target.baseText != (string) args.NewValue) //avoid undo stack overflow
                        target.baseText = (string) args.NewValue;
                })
                );

        public new string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        internal string baseText { get { return base.Text; } set { base.Text = value; } }

        protected override void OnTextChanged(EventArgs e)
        {
            SetCurrentValue(TextProperty, baseText);
            RaisePropertyChanged("Text");
            base.OnTextChanged(e);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
