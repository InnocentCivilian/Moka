using System;
using Grpc.Core;
using Moka.Server.Models;

namespace Moka.Server.Events
{
    public class UserEvents
    {
        public event EventHandler<UserOnlineEventArgs> UserOnlined;
        public event EventHandler<UserModel> UserOfflined;
        public virtual void OnUserOnlined(UserOnlineEventArgs eventArgs)
        {
            UserOnlined?.Invoke(this,eventArgs);
        }
        public virtual void OnUserOfflined(UserModel eventArgs)
        {
            UserOfflined?.Invoke(this,eventArgs);
        }
    }

    public class UserOnlineEventArgs
    {
        public UserModel User { get; set; }
        public IServerStreamWriter<Message> StreamWriter { get; set; }
    }
    
}