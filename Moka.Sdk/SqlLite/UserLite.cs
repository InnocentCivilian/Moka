using System;
using System.Collections.Generic;

namespace Moka.Sdk.SqlLite
{
    public class UserLite
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string NickName { get; set; }
        public string Username { get; set; }

        public UserLite(Guid guid, string nickName, string username)
        {
            Guid = guid;
            NickName = nickName;
            Username = username;
        }

        public static UserLite FromUser(User user)
        {
            return new UserLite(
                Guid.Parse(user.Id),
                user.Nickname,
                user.Username
            );
        }
    }
}