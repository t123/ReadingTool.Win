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
using NPoco;
using RTWin.Annotations;
using RTWin.Entities;

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
                if (string.IsNullOrWhiteSpace(value))
                {
                    _errors["Username"] = true;
                    throw new ValidationException();
                }

                _errors["Username"] = false;
                _username = value;
                OnPropertyChanged("Username");
            }
        }

        public DateTime? LastLogin { get; set; }
        public string AccessKey { get; set; }
        public string AccessSecret { get; set; }
        public bool SyncData { get; set; }
    }
}
