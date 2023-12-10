using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using Microsoft.Build.Tasks;

namespace Server
{
    internal class DBController
    {
        private BuilderContext db;

        public DBController()
        {
            db = new BuilderContext();
        }

        public int GetLength()
        {
            var builders = db.Builders.ToList();
            return (builders.Count + 1);
        }

        public string GetAllBuilders()
        {
            using (var dbContext = new BuilderContext())
            {
                List<Builder> builders = dbContext.Builders.ToList();
                return JsonConvert.SerializeObject(builders);
            }
        }
        public string GetOneBuilderForAll(int id)
        {
            var builder = db.Builders.Find(id);
            if (builder != null)
            {
                return JsonConvert.SerializeObject(builder);
            }
           else { return null; }
        }
        public string GetOneBuilder(int id)
        {
            var builder = db.Builders.Find(id);
            if (builder != null)
            {
                return JsonConvert.SerializeObject(builder);
            }
            else
            {
                return JsonConvert.SerializeObject(new { Message = "Такой записи нет" });
            }
        }

        public string AddBuilder(Builder builder)
        {
            db.Builders.Add(builder);
            db.SaveChanges();
            return JsonConvert.SerializeObject(builder);
        }
        public string RemoveBuilder(int id)
        {
            var builder = db.Builders.Find(id);
            if(builder != null) 
            {
                db.Builders.Remove(builder);
                db.SaveChanges();
                return JsonConvert.SerializeObject(new { Message = "Запись была удалена" });
            }
            else
            {
                return JsonConvert.SerializeObject(new {Message = "Такой записи нет" });
            }
        }
        public string DeleteAndLoadData(List<Builder> builders)
        {
            using (var context = new BuilderContext())
            {

                var allEntities = context.Builders.ToList();
                context.Builders.RemoveRange(allEntities);
                context.Builders.AddRange(builders);
                context.SaveChanges();
                return JsonConvert.SerializeObject(builders);
            }
        }

    }

}
