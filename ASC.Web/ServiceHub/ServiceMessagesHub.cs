using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASC.Models.Models;
using Microsoft.AspNetCore.SignalR;

namespace ASC.Web.ServiceHub
{
    public class ServiceMessagesHub : Hub
    {
        public async Task SendMessage(object message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
