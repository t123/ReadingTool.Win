using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using RTWin.Entities;

namespace RTWin.Controls
{
    public interface IReadingWindow
    {
        void Play();
        void Pause();
        bool IsPlaying();
    }
}
