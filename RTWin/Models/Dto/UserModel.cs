using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using Ninject;
using NPoco;
using RTWin.Annotations;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Models.Dto
{
    public class UserModel : BaseDtoModel
    {
        private string _username;

        public long UserId { get; set; }

        public string Username
        {
            get { return _username; }
            set
            {
                const string field = "Username";
                if (string.IsNullOrWhiteSpace(value))
                {
                    _errors[field] = true;
                    throw new ValidationException();
                }

                _errors[field] = false;
                _username = value;
                OnPropertyChanged(field);
            }
        }

        public DateTime? LastLogin { get; set; }
        public string AccessKey { get; set; }
        public string AccessSecret { get; set; }
        public bool SyncData { get; set; }

        public User ToUser()
        {
            var userService = App.Container.Get<UserService>();
            var u = userService.FindOne(this.UserId);

            if (u == null)
            {
                u = User.NewUser();
            }

            u.AccessKey = this.AccessKey;
            u.AccessSecret = this.AccessSecret;
            u.SyncData = this.SyncData;
            u.Username = this.Username;

            return u;
        }
    }
}
