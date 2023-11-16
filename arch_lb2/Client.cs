using Server;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();

        }
    }
    class Client
    {
        private IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8001);
        private UdpClient client;

        public Client()
        {
            client = new UdpClient();
            client.Connect(serverEP);
            StartAsync().Wait(); // Дождитесь завершения StartAsync перед завершением конструктора
        }

        private async Task StartAsync()
        {
            Console.WriteLine("Введите сообщение (или 'exit' для выхода):");

            while (true)
            {
                await SendRequestAsync("menu");
                string message = await Console.In.ReadLineAsync();

                Console.Clear();

                if (message.ToLower() == "exit")
                {
                    break;
                }

                await SendRequestAsync(message);
            }

            client.Close();
        }
        private Dictionary<string, string> GetRequestParams(RequestType requestType)
        {
            Dictionary<string, string> pars = new Dictionary<string, string>();

            switch (requestType)
            {

                case RequestType.Delete:
                    Console.WriteLine("Введите индекс элемента который хотите удалить: ");
                    pars.Add("Index", Console.ReadLine());
                    break;
                case RequestType.GetOne:
                    Console.WriteLine("Введите индекс элемента который хотите получить: ");
                    pars.Add("Index", Console.ReadLine());
                    break;
                case RequestType.Post:
                    Console.WriteLine("Введите необходимые для создания элемента параметры: ");

                    Console.WriteLine("Введите название застройщика: ");
                    pars.Add("Name", Console.ReadLine());
                    Console.WriteLine("Введите город застройщика: ");
                    pars.Add("City", Console.ReadLine());
                    Console.WriteLine("Введите количество объектов у застройщика: ");
                    pars.Add("NumberOfObjects", Console.ReadLine());
                    Console.WriteLine("Введите количество рабочих у застройщика: ");
                    pars.Add("NumberOfWorkers", Console.ReadLine());
                    Console.WriteLine("Укажите есть ли у застройщика заказ(true/false): ");
                    pars.Add("HasOrder", Console.ReadLine());
                    break;
            }
            return pars;
        }
        private RequestType GetRequestType(string message)
        {
            switch (message.Trim().ToLower())
            {
                case "delete":
                    return RequestType.Delete;
                case "getall":
                    return RequestType.GetAll;
                case "getone":
                    return RequestType.GetOne;
                case "menu":
                    return RequestType.Menu;
                case "post":
                    return RequestType.Post;
                default:
                    return RequestType.Uncorrect;
            }
        }
        private async Task SendRequestAsync(string message)
        {
            RequestType requestType = GetRequestType(message);
            var parametrs = GetRequestParams(requestType);
            Request request = new Request(message, requestType, parametrs);

            string jsonRequest = request.Serialize();
            byte[] requestData = Encoding.UTF8.GetBytes(jsonRequest);
            await client.SendAsync(requestData, requestData.Length);
            await ReceiveResponseAsync();
        }

        private async Task ReceiveResponseAsync()
        {
            UdpReceiveResult result = await client.ReceiveAsync();
            string jsonResponse = Encoding.UTF8.GetString(result.Buffer);
            Response response = Response.Deserialize(jsonResponse);
            Console.WriteLine($"{response.Message}");
        }
    }
}