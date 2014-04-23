using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using NPoco;
using RTWin.Entities;

namespace RTWin.Services
{
    public class DbSettingService
    {
        private readonly Database _db;

        public DbSettingService(Database db)
        {
            _db = db;
        }

        private Dictionary<string, string> GetAndCacheSettings()
        {
            ObjectCache cache = MemoryCache.Default;

            Dictionary<string, string> settings = (Dictionary<string, string>)cache.Get("DbSetttings");

            if (settings == null)
            {
                settings = _db.Fetch<DbSetting>().ToDictionary(x => x.Key.ToLowerInvariant(), x => x.Value);
                cache.Add("DbSettings", settings, ObjectCache.InfiniteAbsoluteExpiration);
            }

            return settings;
        }

        public void Save(DbSetting dbSetting)
        {
            if (dbSetting == null)
            {
                return;
            }

            Save(dbSetting.Key, dbSetting.Value);
        }

        public void Save(string key, object value)
        {
            key = NormalizeKey(key);
            var setting = _db.FetchWhere<DbSetting>(x => x.Key == key).FirstOrDefault();

            if (setting == null)
            {
                setting = new DbSetting()
                {
                    Key = key,
                    Value = (value ?? "").ToString()
                };

                _db.Insert(setting);
            }
            else
            {
                setting.Value = (value ?? "").ToString();
                _db.Update(setting);
            }

            ObjectCache cache = MemoryCache.Default;
            cache.Remove("DbSettings");
        }

        public Dictionary<string, string> Get()
        {
            return GetAndCacheSettings();
        }

        public string Get(string key)
        {
            key = NormalizeKey(key);
            var setting = GetAndCacheSettings().FirstOrDefault(x => x.Key == key);
            return setting.Value;
        }

        public T Get<T>(string key)
        {
            key = NormalizeKey(key);
            var settings = GetAndCacheSettings();
            var setting = settings.ContainsKey(key) ? settings[key] : null;

            Type t = typeof(T);
            t = Nullable.GetUnderlyingType(t) ?? t;

            return (setting == null) ? default(T) : (T)Convert.ChangeType(setting, t);
        }

        private string NormalizeKey(string key)
        {
            return (key ?? "").ToLowerInvariant().Trim();
        }
    }
}
