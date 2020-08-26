using System;
using Moka.Server.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Moka.Server.Data
{
    public class UserData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public DateTime Created_at { get; set; }

        public UserData(Guid guid, string name, string password, DateTime createdAt)
        {
            Guid = guid;
            Name = name;
            Password = password;
            Created_at = createdAt;
        }

        public static UserData FromUser(User user)
        {
            return new UserData(user.Guid, user.Name, user.Password, user.Created_at);
        }

        public User ToUser()
        {
            return new User(Guid, Name, Password, Created_at);
        }
    }
    
}