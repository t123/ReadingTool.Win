using System;
using Ninject;
using NPoco;
using RTWin.Core.Enums;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Models.Dto
{
    public class DbSettingModel : BaseDtoModel
    {
        private string _key;
        private string _value;

        public string Key
        {
            get { return _key; }
            set
            {
                if (value == _key) return;
                _key = value;
                OnPropertyChanged();
            }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                if (value == _value) return;
                _value = value;
                OnPropertyChanged();
            }
        }

        public DbSetting ToDbSetting()
        {
            var settingService = App.Container.Get<DbSettingService>();
            var setting = settingService.FindOne(Key);

            if (setting == null)
            {
                setting = new DbSetting()
                {
                    Key = Key,
                    Value = Value
                };

                return setting;
            }

            setting.Value = Value;
            return setting;
        }
    }
}