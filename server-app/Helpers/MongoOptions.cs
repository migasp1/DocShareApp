using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocShareApp.Helpers
{
    public class MongoOptions
    {
        public string MongoDbConnectionString {get; set;}
        public string Database { get; set; }
        public bool UseMongoDb { get; set; }
    }
}
