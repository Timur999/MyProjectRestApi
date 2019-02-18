using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections;

namespace MyProjectRestApi.Models
{
    [HubName("NotifyHub")]
    public class NotifyHub : Hub
    {
        public static class UserHandler
        {
            public static Hashtable htConnectionClients = new Hashtable();

        }

        public void RegisterUser(string currentUserId)
        {
            UserHandler.htConnectionClients[currentUserId] = Context.ConnectionId;
        }

        public void SendMessage(MessageDTO message)
        {
            
            foreach (var key in UserHandler.htConnectionClients.Keys)
            {
                foreach(string UserReceiverId in message.ListUserReceiver)
                {
                    if (key.ToString() == UserReceiverId)
                    {
                        Clients.Client(UserHandler.htConnectionClients[key].ToString())
                            .OnMessageSent(message);
                    }
                }
            }

            Clients.Caller.OnMessageSent(message);
        }
        //UserId, message.MessageText
        //Clients.All.OnMessageSent(message);

        //Clients.All.addNewMessageToPage(message);
        // string n = Context.User.Identity.Name;

    }
}