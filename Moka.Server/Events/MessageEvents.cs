using System;
using Moka.Server.Models;
using Moka.Server.Service;

namespace Moka.Server.Events
{
    public class MessageEvents
    {
        // 1- define delegate
        //2- define method based on that event
        // 3- raise the event
        // public delegate void MessageReceivedEventHandler(object source, EventArgs args);
        //
        // public event MessageReceivedEventHandler MessageReceived;
        public event EventHandler<Message> MessageReceived;
        public virtual void OnMessageReceived(Message message)
        {
            MessageReceived?.Invoke(this,message);
        }
    }
    
}