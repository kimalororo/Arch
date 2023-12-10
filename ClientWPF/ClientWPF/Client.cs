using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientWPF.ViewModels;
using ClientWPF.Command;
using ClientWPF;

namespace Client
{
    class Client : ViewModel
    {

        private IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8001);
        private UdpClient client;
        private ObservableCollection<Builder> builders;
        private string _serverResponse;

        private Command getAllCommand;
        private Command addCommand;
        private Command deleteCommand;
        private Command saveCommand;

        public Command GetAllCommand
        {
            get
            {
                return getAllCommand ?? (getAllCommand = new Command(obj =>
                {
                    _ = SendRequestAsync(new Request(" ", RequestType.GetAll, new Dictionary<string, string>()));
                }));
            }
        }

        public Command DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new Command(obj => this.RemoveBulder());
                }
                return deleteCommand;
            } 
        }
        public Command SaveCommand
        {
            get
            {
                return saveCommand ?? (saveCommand = new Command(obj =>
                {
                    try
                    {
                        var jsonString = JsonConvert.SerializeObject(Builders);
                        _ = SendRequestAsync(new Request(jsonString, RequestType.UpdateTable, new Dictionary<string, string>()));
                    }
                    catch (Exception ex)
                    {
                        _serverResponse = ex.Message;
                    }
                }));
            }
        }

        public ObservableCollection<Builder> Builders
        {
            get { return builders; }
            set
            {
                Set(ref builders, value);
            }
        }

        public string ServerResponse
        {
            get => _serverResponse;
            set
            {
                Set(ref _serverResponse, value, nameof(_serverResponse));
            }
        }
            
        public Client()
        {
            builders = new ObservableCollection<Builder>();
            client = new UdpClient();
            Thread.Sleep(1000);
            client.Connect(serverEP);
        }
         public Builder SelectedBuilder
        {
            get { return selectedBuilder; }
            set
            {
                selectedBuilder = value;
                OnPropertyChanged("SelectedBuilder");
            }
        }
        private Builder selectedBuilder= null;
        void RemoveBulder()
        {
            if (SelectedBuilder != null)
            {
                Builders.Remove(SelectedBuilder);
                SelectedBuilder = null;
            }

        }

        private async Task SendRequestAsync(Request request)
        {
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
            try
            {
                JToken jsonToken = JToken.Parse(response.Message);
                if (jsonToken is JArray)
                {
                    Builders = new ObservableCollection<Builder>(jsonToken.ToObject<List<Builder>>());
                }
                else if (jsonToken is JObject)
                {
                    Builders.Add(jsonToken.ToObject<Builder>());
                }
            }
            catch (JsonReaderException ex)
            {
                ServerResponse = response.Message;
            }
        }
    }
};