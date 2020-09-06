using System;
using System.Collections.Generic;
using Moka.Server.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OtpNet;

namespace Moka.Server.Data
{
    
    public class UserData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public Guid Guid { get; set; }
        public string NickName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime Created_at { get; set; }

        public List<Device> Devices { get; set; }
        public UserData(Guid guid, string nickName, string username, string password, DateTime createdAt, List<Device> devices)
        {
            Guid = guid;
            NickName = nickName;
            Username = username;
            Password = password;
            Created_at = createdAt;
            Devices = devices;
        }

        public static UserData FromUser(UserModel user)
        {
            return new UserData(user.Guid, user.NickName, user.UserName, user.Password, user.Created_at,user.Devices);
        }

        public UserModel ToUserModel()
        {
            return new UserModel(Guid, Username, Devices, NickName, Password, Created_at);
        }
    }
}