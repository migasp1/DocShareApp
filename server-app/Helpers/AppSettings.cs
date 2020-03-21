using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocShareApp.Helpers
{
    public class AppSettings
    {
        //Used in appsettings.json to sign and verify JWT tokens
        public string Secret { get; set; }
    }
}
