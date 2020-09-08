using System;

namespace Moka.Sdk.SqlLite
{
    public class MessageLite
    {
        public  int Id { get; set; }

        public Guid LocalId { get; set; }


        public byte[] Data { get; set; }
        public MessageType MessageType { get; set; }
        public Guid From { get; set; }
        public Guid To { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime? Delivered_at { get; set; }
        public DateTime? Read_at { get; set; }
    }
}