using System;
using System.Net;
using System.Collections.Generic;

namespace SMT.HRM.BLL
{
    public class AppConfig 
    {
        public LoginUser CurrentUser { get; set; }

        public AppConfig()
        {
            CurrentUser = new LoginUser();
        }
    }
}
