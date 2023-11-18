using Client;
using NLog;
using NLog.Config;
using System;
using System.Net;
using System.Net.Sockets;
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
            private DBController builderController;
            private int port;
            private UdpClient listener;
            private readonly Logger Logger;

            public Server(int _port)
            {
                Logger = LogManager.GetCurrentClassLogger();
                port = _port;
                listener = new UdpClient(_port);
                builderController = new DBController();
                Logger.Info("Сервер запущен.");
            
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
                    Logger.Fatal(ex);
                    await Console.Out.WriteLineAsync("Error");
                    await Console.Out.WriteLineAsync(ex.ToString());
                }
            }


            private void ProcessRequest(byte[] requestData, IPEndPoint clientEndPoint)
            {
                StringBuilder message = new StringBuilder(" Запрос получен"); ;
                ResponseType responseType = ResponseType.Success;
                string jsonRequest = Encoding.UTF8.GetString(requestData);
                Request request = Request.Deserialize(jsonRequest);


                Console.Out.WriteLineAsync($"{request.MessageType}:{request.Message}");
                switch (request.MessageType)
                {
                    case RequestType.Delete:
                        try
                        {
                            if (request.Parametrs.TryGetValue("Index", out string indexString) && int.TryParse(indexString, out int index))
                            {
                                Logger.Info("Запись удалена.");
                                message = new StringBuilder(builderController.RemoveBuilder(index));
                                responseType = ResponseType.Success;
                            }
                            else
                            {
                                Logger.Error($"Ошибка: Некорректный ID. Индекс = {indexString}");
                                message = new StringBuilder("Ошибка: Некорректный формат индекса.");
                                responseType = ResponseType.Error;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Fatal(ex);

                            message = new StringBuilder($"Ошибка: Некорректный индекс");
                            responseType = ResponseType.Error;
                        }
                        break;

                    case RequestType.GetAll:
                        try
                        {
                            StringBuilder newlist = new StringBuilder();
                            for (int i = 1; i <= builderController.GetLength(); i++)
                            {
                                string list = null;
                                list = builderController.GetOneBuilderForAll(i);
                                if (list != null)
                                {
                                    newlist.AppendLine(list);
                                }
                            }
                            message = newlist;
                        }
                        catch (Exception ex)
                        {
                            Logger.Fatal(ex);
                            message = new StringBuilder($"Ошибка: {ex}");
                            responseType = ResponseType.Error;
                        }
                        break;
                    case RequestType.GetOne:
                        try
                        {
                            if (request.Parametrs.TryGetValue("Index", out string indexString) && int.TryParse(indexString, out int index))
                            {
                                var one = builderController.GetOneBuilder(index);
                                message = new StringBuilder(one);
                                responseType = ResponseType.Success;
                                
                            }
                            else
                            {
                                message = new StringBuilder("Ошибка: Некорректный формат индекса.");
                                responseType = ResponseType.Error;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Fatal(ex);

                            message = new StringBuilder($"Ошибка: Некорректный индекс");
                            responseType = ResponseType.Error;
                        }
                        break;
                    case RequestType.Add:
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
                                var ad = builderController.AddBuilder(newBuilder);
                                message = new StringBuilder(ad);
                                responseType = ResponseType.Success;
                            }
                            else
                            {
                                Logger.Error($"Ошибка: Некорректные параметры запроса. {request.Parametrs.ToString()}");
                                message = new StringBuilder("Ошибка: Некорректные параметры запроса.");
                                responseType = ResponseType.Error;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Fatal(ex);

                            message = new StringBuilder($"Ошибка: {ex.Message}");
                            responseType = ResponseType.Error;
                        }
                        break;

                    case RequestType.Menu:
                        message = new StringBuilder("Список команд:\n- getOne\n- getAll\n- add\n- delete\n- exit");
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