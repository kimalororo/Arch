using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Server
{
    class BuilderContext:DbContext
    {
        public BuilderContext() : base("DBConnection")
        {
        }
        public DbSet<Builder> Builders { get; set; }
    }
}
