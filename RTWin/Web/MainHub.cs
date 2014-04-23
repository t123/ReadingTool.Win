using System.Windows;
using System.Windows.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using Ninject;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Web
{
    public class MainHub
    {
        private HubConnection _hubConnection;
        private IHubProxy _mainHubProxy;
        private static MainHub _hub;

        private MainHub()
        {
            var settings = App.Container.Get<DbSettingService>();
            _hubConnection = new HubConnection(settings.Get<string>(DbSetting.Keys.BaseWebSignalRAddress));
            _mainHubProxy = _hubConnection.CreateHubProxy("MainHub");
            InitHub();
        }

        public static void Init()
        {
            _hub = new MainHub();
        }

        private void InitHub()
        {
            _mainHubProxy.On<string, string>("addMessage", (element, action) =>
            {
                if (element == "video")
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        double time;
                        MainWindow owner = System.Windows.Application.Current.MainWindow as MainWindow;

                        if (owner == null)
                        {
                            return;
                        }

                        if (!double.TryParse(action, out time) || owner.MainWindowViewModel == null)
                        {
                            _mainHubProxy.Invoke("Send", new object[] { "srtl1", -1 });
                            _mainHubProxy.Invoke("Send", new object[] { "srtl2", -1 });
                        }
                        else
                        {
                            //var sub = owner.MainWindowViewModel.GetSub(time);
                            //_mainHubProxy.Invoke("Send", new object[] { "srtl1", sub.Item1 });
                            //_mainHubProxy.Invoke("Send", new object[] { "srtl2", sub.Item2 });
                        }
                    });
                }
                else if (element == "modal")
                {
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() => Clipboard.SetText(action, TextDataFormat.UnicodeText), DispatcherPriority.Background);
                }
            });

            _hubConnection.Start();
        }
    }


    //public class MainHub : Hub
    //{
    //    public void Send(string element, string action)
    //    {
    //        Clients.All.addMessage(element, action);
    //    }
    //}
}