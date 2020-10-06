using System;
using System.Collections.Generic;
using System.Text;

namespace Task13.DbWork.Model
{
    class AppConfig
    {
        public string DbHost { get; set; }
        public ConfigurationObjeckt SuperUser { get; set; }
        public ConfigurationObjeckt User { get; set; }
    }
}
