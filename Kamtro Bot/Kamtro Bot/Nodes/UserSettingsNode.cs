﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Nodes
{
    public class UserSettingsNode
    {
        public string Username;
        public bool TitleNotify;
        public bool AllNotify;

        public UserSettingsNode(string username) {
            Username = username;

            TitleNotify = true;
            AllNotify = true;
        }

        public bool this[string name] {
            get {
                PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo property in properties) {
                    if (property.Name == name && property.CanRead)
                        return (property.GetValue(this) as bool?).Value;
                }

                throw new ArgumentException("Can't find property");
            }

            set {
                return;
            }
        }
    }
}
