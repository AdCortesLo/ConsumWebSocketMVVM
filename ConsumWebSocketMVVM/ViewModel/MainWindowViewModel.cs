using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsumWebSocketMVVM.ViewModel
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        CancellationTokenSource cts = new CancellationTokenSource();
        ClientWebSocket socket = new ClientWebSocket();

        public RelayCommand<string> BtsCommand { get; set; }

        public MainWindowViewModel()
        {
            Visible = "Visible";
            BtsCommand = new RelayCommand<string>(BtsClick);
        }

        private async void BtsClick(string btName)
        {
            switch (btName)
            {
                case "GuardarUsuario":
                    string nom = Usuari;

                    string wsUri = string.Format("wss://localhost:44391/api/websocket?nom={0}", nom);
                    await socket.ConnectAsync(new Uri(wsUri), cts.Token);
                    
                    TaskFactoryStartNew(cts, socket);
                    Visible = "Hidden";
                    
                    break;

                case "Enviar":
                    if (Visible.Equals("Hidden"))
                    {
                        var missatge = Mensaje;
                        if (missatge == "Adeu")
                        {
                            cts.Cancel();
                            System.Environment.Exit(0);
                            return;
                        }
                        byte[] sendBytes = Encoding.UTF8.GetBytes(missatge);
                        var sendBuffer = new ArraySegment<byte>(sendBytes);
                        await socket.SendAsync(sendBuffer, WebSocketMessageType.Text, endOfMessage: true, cancellationToken: cts.Token);
                        Mensaje = "";
                    }
                    break;
            }
        }

        private void TaskFactoryStartNew(CancellationTokenSource cts, ClientWebSocket socket)
        {
            Task.Factory.StartNew(
                            async () =>
                            {
                                var rcvBytes = new byte[128];
                                var rcvBuffer = new ArraySegment<byte>(rcvBytes);
                                while (true)
                                {
                                    WebSocketReceiveResult rcvResult = await socket.ReceiveAsync(rcvBuffer, cts.Token);
                                    byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                                    string rcvMsg = Encoding.UTF8.GetString(msgBytes);
                                    ContenidoChat += rcvMsg + "\n";
                                }
                            }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private string _visible;
        public string Visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
                NotifyPropertyChanged();
            }
        }

        private string _contenidoChat;
        public string ContenidoChat
        {
            get { return _contenidoChat; }
            set
            {
                _contenidoChat = value;
                NotifyPropertyChanged();
            }
        }

        private string _mensaje;
        public string Mensaje
        {
            get { return _mensaje; }
            set
            {
                _mensaje = value;
                NotifyPropertyChanged();
            }
        }

        private string _usuari;
        public string Usuari
        {
            get { return _usuari; }
            set
            {
                _usuari = value;
                NotifyPropertyChanged();
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
