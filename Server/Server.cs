using Client;
using NLog;
using NLog.Config;
using System;
using System.ComponentModel.Design;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            LogManager.Configuration = new XmlLoggingConfiguration("NLog.config");

            Server server = new Server(8001);
            Console.WriteLine("Сервер запущен. Для выхода нажмите Enter.");
            Console.ReadLine(); // Ждем, пока пользователь нажмет Enter, прежде чем завершить программу

        }
        class Server
        {
            private BuilderController builderController;
            private int port;
            private UdpClient listener;
            private readonly Logger Logger;

            public Server(int _port)
            {
                Logger = LogManager.GetCurrentClassLogger();

                port = _port;
                listener = new UdpClient(_port);
                builderController = new BuilderController();
                Logger.Info("Сервер запущен.");

                builderController.ReadAllRecords();
                Task.Run(() => StartListenAsync());
            }

            private async Task StartListenAsync()
            {
                try
                {
                    while (true)
                    {
                        UdpReceiveResult result = await listener.ReceiveAsync();

                        if (result.Buffer != null && result.Buffer.Length > 0)
                        {
                            ProcessRequest(result.Buffer, result.RemoteEndPoint);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }


            private void ProcessRequest(byte[] requestData, IPEndPoint clientEndPoint)
            {
                StringBuilder message = new StringBuilder(" Запрос получен"); ;
                ResponseType responseType = ResponseType.Success;
                // Десериализация запроса
                string jsonRequest = Encoding.UTF8.GetString(requestData);
                Request request = Request.Deserialize(jsonRequest);

                // Обработка запроса (ваша логика здесь)
                // ...
                Console.Out.WriteLineAsync($"{request.MessageType}:{request.Message}");
                switch (request.MessageType)
                {
                    case RequestType.Delete:
                        try
                        {
                            if (request.Parametrs.TryGetValue("Index", out string indexString) && int.TryParse(indexString, out int index))
                            {
                                builderController.RemoveRecord(index);
                                message = new StringBuilder("Запись успешно удалена.");
                                responseType = ResponseType.Success;
                                builderController.WriteRecords();
                            }
                            else
                            {
                                message = new StringBuilder("Ошибка: Некорректный формат индекса.");
                                responseType = ResponseType.Error;
                            }
                        }
                        catch (IndexOutOfRangeException ex)
                        {
                            Logger.Error(ex);

                            message = new StringBuilder($"Ошибка: Некорректный индекс");
                            responseType = ResponseType.Error;
                        }
                        break;
                    case RequestType.GetAll:
                        message = new StringBuilder();
                        var list = builderController.GetBuilders();
                        for (int index = 0; index < list.Count; index++)
                        {
                            message.Append($"Индекс : {index}" +
                                $"\nНазвание : {list[index].Name}" +
                                $"\nГород : {list[index].City}" +
                                $"\nКоличество объектов : {list[index].NumberOfObjects}" +
                                $"\nКоличество рабочих : {list[index].NumberOfWorkers}" +
                                $"\nНаличие заказа : {list[index].HasOrder}" +
                                $"\n----------------------------------\n");
                        }

                        break;
                    case RequestType.GetOne:
                        try
                        {
                            if (request.Parametrs.TryGetValue("Index", out string indexString) && int.TryParse(indexString, out int index))
                            {
                                var el = builderController.GetBuilder(index);
                                message = new StringBuilder($"Индекс : {index}" +
                                $"\nНазвание : {el.Name}" +
                                $"\nГород : {el.City}" +
                                $"\nКоличество объектов : {el.NumberOfObjects}" +
                                $"\nКоличество рабочих : {el.NumberOfWorkers}" +
                                $"\nНаличие заказа : {el.HasOrder}" +
                                $"\n----------------------------------");
                                responseType = ResponseType.Success;
                                builderController.WriteRecords();
                            }
                            else
                            {
                                message = new StringBuilder("Ошибка: Некорректный формат индекса.");
                                responseType = ResponseType.Error;
                            }
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            Logger.Error(ex);

                            message = new StringBuilder($"Ошибка: Некорректный индекс");
                            responseType = ResponseType.Error;
                        }
                        break;
                    case RequestType.Post:
                        try
                        {
                            string name, address;
                            int numberOfObjects, numberOfWorkers;
                            bool hasOrder;

                            if (request.Parametrs.TryGetValue("Name", out name) &&
                                request.Parametrs.TryGetValue("City", out address) &&
                                request.Parametrs.TryGetValue("NumberOfObjects", out string numberOfObjectsStr) &&
                                int.TryParse(numberOfObjectsStr, out numberOfObjects) &&
                                request.Parametrs.TryGetValue("NumberOfWorkers", out string numberOfWorkersStr) &&
                                int.TryParse(numberOfWorkersStr, out numberOfWorkers) &&
                                request.Parametrs.TryGetValue("HasOrder", out string hasOrderStr) &&
                                bool.TryParse(hasOrderStr, out hasOrder))
                            {
                                Builder newBuilder = new Builder
                                {
                                    Name = name,
                                    City = address,
                                    NumberOfObjects = numberOfObjects,
                                    NumberOfWorkers = numberOfWorkers,
                                    HasOrder = hasOrder
                                };
                                builderController.AddRecord(newBuilder);
                                message = new StringBuilder("Запись успешно добавлена.");
                                responseType = ResponseType.Success;
                                builderController.WriteRecords();
                            }
                            else
                            {
                                message = new StringBuilder("Ошибка: Некорректные параметры запроса.");
                                responseType = ResponseType.Error;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);

                            message = new StringBuilder($"Ошибка: {ex.Message}");
                            responseType = ResponseType.Error;
                        }
                        break;

                    case RequestType.Menu:
                        message = new StringBuilder("Список команд:\n- delete\n- getAll\n- getOne\n- post\n- exit");
                        break;
                    case RequestType.Uncorrect:
                        responseType = ResponseType.Error;
                        message = new StringBuilder("неверная команда");
                        break;


                }

                ServerResponse(message, responseType, clientEndPoint);
            }

            private void ServerResponse(StringBuilder message, ResponseType respType, IPEndPoint clientEndPoint)
            {
                // Подготовка ответа
                Response response = new Response(message.ToString(), respType);
                string jsonResponse = response.Serialize();
                byte[] responseData = Encoding.UTF8.GetBytes(jsonResponse);

                // Отправка ответа асинхронно
                listener.SendAsync(responseData, responseData.Length, clientEndPoint);
            }
        }
    }
}