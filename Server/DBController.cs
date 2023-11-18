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

        //public string GetAllBuilders()
        // {
        //     StringBuilder builders = new StringBuilder();
        //     for (int i = 0; i <= GetLength(); i++)
        //         builders.Append(db.Builders.Find(i));
        //     return builders.ToString();
        // }
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
    }   

}
