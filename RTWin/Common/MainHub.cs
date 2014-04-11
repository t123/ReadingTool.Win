using Microsoft.AspNet.SignalR;

namespace RTWin.Common
{
    public class MainHub : Hub
    {
        public void Send(string element, string action)
        {
            Clients.All.addMessage(element, action);
        }
    }
}