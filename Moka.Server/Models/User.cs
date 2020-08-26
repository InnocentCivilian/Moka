using System;

namespace Moka.Server.Models
{
    public class User
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public DateTime Created_at { get; set; }

        public User(Guid guid, string name, string password, DateTime createdAt)
        {
            Guid = guid;
            Name = name;
            Password = password;
            Created_at = createdAt;
        }

        //todo:active devices
        //todo:keys
    }
}