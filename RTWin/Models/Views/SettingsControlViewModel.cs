using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using AutoMapper;
using GalaSoft.MvvmLight.Command;
using RTWin.Entities;
using RTWin.Models.Dto;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class SettingsControlViewModel : BaseViewModel
    {
        private readonly DbSettingService _settings;

        private IEnumerable<DbSettingModel> _dbSettings;
        private ICommand _saveCommand;

        public ICommand SaveCommand
        {
            get { return _saveCommand; }
            set { _saveCommand = value; }
        }

        public ObservableCollection<DbSettingModel> DbSettings
        {
            get
            {
                if (_dbSettings == null)
                {
                    _dbSettings = Mapper.Map<IEnumerable<DbSetting>, IEnumerable<DbSettingModel>>(_settings.FindAll());
                }

                return new ObservableCollection<DbSettingModel>(_dbSettings);
            }
            set
            {
                _dbSettings = value;
                OnPropertyChanged("DbSettings");
            }
        }

        public SettingsControlViewModel(DbSettingService settings)
        {
            _settings = settings;

            _saveCommand = new RelayCommand<DataGridCellEditEndingEventArgs>(param =>
            {
                if (param.Cancel)
                {
                    return;
                }

                var item = param.Row.Item as DbSettingModel;
                _settings.Save(item.Key,  ((TextBox) param.EditingElement).Text);
                DbSettings = null;
            });
        }
    }
}
