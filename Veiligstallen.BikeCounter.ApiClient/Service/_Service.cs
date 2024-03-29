﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Veiligstallen.BikeCounter.ApiClient
{
    public partial class Service
    {
        public const string ANONYMOUS = "anonymous";
        private static readonly string ANONYMOUS_BASE_AUTH_HDR = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{ANONYMOUS}:{ANONYMOUS}"));

        private readonly Configuration _cfg;
        private readonly string _user;
        private readonly string _pass;
        private readonly string _authHeader;

        public Service()
        {
            _cfg = Configuration.Read();
            _user = _cfg.User;
            _pass = _cfg.Pass;
        }

        public Service(string user, string pass)
            : this()
        {
            _user = user;
            _pass = pass;
        }

        public Service(string authHeader)
            : this()
        {
            _authHeader = authHeader;
        }
    }
}
