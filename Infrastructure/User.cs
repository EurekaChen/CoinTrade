using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eureka.CoinTrade.Base;

namespace Eureka.CoinTrade.Infrastructure
{
    public class User :BindableBase
    {       

        private string firstName;
        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {
                SetProperty(ref firstName, value);           
            }
        }

        private string lastName;
        public string LastName
        {
            get
            {
                return lastName;
            }
            set
            {
                SetProperty(ref lastName, value);
            }
        }

        //已经遵循了.net命名规范，参见书本39页！
        private string email;
        public string Email
        {
            get
            {
                return email;
            }
            set
            {
                SetProperty(ref email, value);
            }
        }

        private string mobile;
        public string Mobile
        {
            get
            {
                return mobile;
            }
            set
            {
                SetProperty(ref mobile, value);
            }
        }

        private string instantMessage;
        public string InstantMessage
        {
            get
            {
                return instantMessage;
            }
            set
            {
                SetProperty(ref instantMessage, value);
            }
        }     

    }
}
