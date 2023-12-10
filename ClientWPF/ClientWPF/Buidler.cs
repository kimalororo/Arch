using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Server
{

    internal class Builder
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public int NumberOfObjects { get; set; }
        public int NumberOfWorkers { get; set; }
        public bool HasOrder { get; set; }
        public Builder()
        {
            ID = 0;
            Name = "Название";
            City = "Город";
            NumberOfObjects = 0;
            NumberOfWorkers = 0;
            HasOrder = false;
        }
    }
}
