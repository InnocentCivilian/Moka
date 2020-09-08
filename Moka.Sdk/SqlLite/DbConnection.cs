using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Moka.Sdk.SqlLite
{
    public interface IDbConnection
    {
    }

    public class DbConnection : IDbConnection
    {
        const string newDatabaseFile = EnvConsts.DEFAULTDBFILE;
        public readonly MokaClientContext _db;

        public DbConnection()
        {
            _db = new MokaClientContext(newDatabaseFile);
            _db.Database.EnsureCreated();
        }

        // public static void Main(string[] args)
        // {
        //     
        //     db.Messages.Add(new MessageLite
        //     {
        //         Created_at = DateTime.Now,
        //         Data = new byte[] { },
        //         Delivered_at = DateTime.Now,
        //         From = Guid.Empty,
        //         To = Guid.Empty,
        //         LocalId = Guid.Empty,
        //         MessageType = MessageType.Text
        //     });
        //     // db.Users.Add(
        //     //     new User
        //     //     {
        //     //         Id = "123",
        //     //         Nickname = "hahaha",
        //     //         Username = "fuckme"
        //     //     });
        //     Console.WriteLine("{0} records saved to database", db.SaveChanges());
        //     Console.WriteLine("{0} records are in message table", db.Messages.Count());
        //     foreach (var messageLite in db.Messages)
        //     {
        //         Console.WriteLine(messageLite.Created_at);
        //     }
        //
        //     //
        //     // // Display all blog URIs from the current database
        //     // Console.WriteLine("All {0} blogs in database:", db.Blogs.Count());
        //     // foreach (var blog in db.Blogs) Console.WriteLine(" - #{0} {1}", blog.BlogId, blog.Url);
        // }
    }
}