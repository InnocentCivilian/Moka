using System;
using System.Collections.Generic;

namespace Moka.Server.Models
{
    

    public class UserModel
    {
        public Guid Guid { get; set; }
        public string NickName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime Created_at { get; set; }

        public List<Device> Devices { get; set; }

        public UserModel(Guid guid, string userName, List<Device> devices = default, string nickName = default,
            string password = default,
            DateTime createdAt = default)
        {
            Guid = guid;
            NickName = nickName;
            Password = password;
            Created_at = createdAt;
            UserName = userName;
            Devices = devices;
        }

        public User ToUser()
        {
            return new User
            {
                Id = Guid.ToString(),
                Nickname = NickName,
                Username = UserName,
            };
        }

        public static UserModel FromUser(User user)
        {
            return new UserModel(
                Guid.Parse(user.Id),
                user.Username,
                new List<Device>(),
                user.Nickname
            );
        }

        //todo:active devices
        //todo:keys
    }
}