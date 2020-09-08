using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Moka.Sdk.SqlLite;

namespace Moka.Sdk.Extenstion
{
    public static class ExtenstionMethods
    {
        public static List<MessageLite> ToMessageLiteList(this RepeatedField<Message> messages)
        {
            var msgs = messages.Select(x => x.ToMessageLite()).ToList();
            return msgs;
        }

        public static MessageLite ToMessageLite(this Message message)
        {
            return new MessageLite
            {
                Created_at = message.CreatedAtTimeStamp.ToDateTime(),
                Data = message.Payload.ToByteArray(),
                Delivered_at = message.DeliveredAtTimeStamp?.ToDateTime(),
                Read_at = message.ReadAtTimeStamp?.ToDateTime(),
                From = Guid.Parse(message.SenderId),
                To = Guid.Parse(message.ReceiverId),
                LocalId = Guid.Parse(message.LocalId),
                MessageType = message.Type,
            };
        }

        public static UserLite ToUserLite(this User user)
        {
            return new UserLite(
                Guid.Parse(user.Id),
                user.Nickname,
                user.Username
            );
        }
    }
}