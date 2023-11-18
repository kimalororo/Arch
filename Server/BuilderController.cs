//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Runtime.InteropServices;

//namespace Server
//{
//    internal class BuilderController
//    {
//        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
//        private string filePath; // Путь к файлу CSV
//        private List<Builder> builders;
//        public BuilderController(string filePath = "C:/Misha/Study/ARCHITECTURE/Projects/builders.csv")
//        {
//            this.filePath = filePath;
//        }

//        // Метод для чтения всех записей из файла и возврата списка объектов Builders
//        public List<Builder> ReadAllRecords()
//        {
//            builders = new List<Builder>();

//            try
//            {
//                if (!File.Exists(filePath))
//                {
//                    WriteRecords();
//                    Console.WriteLine("No");
//                }
//                using (StreamReader reader = new StreamReader(filePath))
//                {
//                    string line;
//                    while ((line = reader.ReadLine()) != null)
//                    {
//                        string[] data = line.Split(',');

//                        // Проверяем, что данные в строке корректны, иначе пропускаем строку
//                        if (data.Length >= 4)
//                        {
//                            Builder builder = new Builder
//                            {
//                                Name = data[0],
//                                City = data[1],
//                                NumberOfObjects = int.Parse(data[2]),
//                                NumberOfWorkers = int.Parse(data[3]),
//                                HasOrder = bool.Parse(data[4])
//                            };

//                            builders.Add(builder);
//                        }
//                    }
//                }

//                Logger.Info("Файл прочитан", builders);
//            }
//            catch (IOException e)
//            {
//                Logger.Error(e.ToString());
//            }

//            return builders;
//        }

//        // Метод для записи списка объектов Builder в файл CSV
//        public void WriteRecords()
//        {
//            try
//            {
//                using (StreamWriter writer = new StreamWriter(filePath))
//                {
//                    foreach (Builder builder in builders)
//                    {
//                        string line = $"{builder.Name},{builder.City},{builder.NumberOfObjects},{builder.NumberOfWorkers},{builder.HasOrder}";
//                        writer.WriteLine(line);
//                    }
//                }
//            }
//            catch (IOException e)
//            {
//                Logger.Error($"Ошибка записи в файл: {e.Message}");
//            }
//            catch (Exception ex)
//            {
//                Logger.Error(ex.ToString());
//            }
//        }

//        // Метод для добавления новой записи в список и сохранения в файле
//        public void AddBuilder(Builder builder)
//        {
//            builders.Add(builder);
//        }

//        // Метод для удаления записи по индексу
//        public void RemoveBuilder(int index)
//        {

//            if (index >= 0 && index < builders.Count)
//            {
//                builders.RemoveAt(index);
//            }
//            else
//            {
//                Logger.Error("Неверный индекс для удаления записи.");
//                throw new IndexOutOfRangeException();
//            }
//        }


//        public List<Builder> GetAllBuilders()
//        {
//            return builders;
//        }
//        public Builder GetOneBuilder(int index)
//        {
//            return builders.ElementAt(index);
//        }
//    }
//}
