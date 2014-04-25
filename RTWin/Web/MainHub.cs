using Microsoft.AspNet.SignalR;

namespace RTWin.Web
{
    public class MainHub : Hub
    {
        public void Send(string element, string action)
        {
            Clients.All.addMessage(element, action);
        }
    }
}