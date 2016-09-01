using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using Eureka.CoinTrade.Base;
using Newtonsoft.Json;

namespace Eureka.CoinTrade.Infrastructure
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RegisterInfo : BindableBase
    {

        private User user;
        public User User
        {
            get
            {
                return user;
            }
            set
            {
                SetProperty(ref user, value);
            }
        }

        private DateTime expireDate;
        public DateTime ExpireDate
        {
            get
            {
                return expireDate;
            }
            set
            {
                SetProperty(ref expireDate, value);
            }
        }

        private string machineCode;
        public string MachineCode
        {
            get
            {
                return machineCode;
            }
            set
            {
                SetProperty(ref machineCode, value);
            }
        }

        private bool isEnabledAll;
        public bool IsEnabledAll
        {
            get
            {
                return isEnabledAll;
            }
            set
            {
                SetProperty(ref isEnabledAll, value);
            }
        }

        private Dictionary<string, Collection<string>> enabledPairs;
        public Dictionary<string, Collection<string>> EnabledPairs
        {
            get
            {
                return enabledPairs;
            }
            set
            {
                SetProperty(ref enabledPairs, value);
            }
        }

    }
}
