﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class ApplicationSettings
    {
        public string JWT_Secret { get; set; }
        public string client_URL { get; set; }
    }
    public class EmailSettings
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
    }
    public class EMailDetails
    {
        public string subject { get; set; }
        public string message { get; set; }      
    }
}
