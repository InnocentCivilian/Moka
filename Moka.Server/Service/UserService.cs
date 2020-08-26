using System;
using System.Threading.Tasks;
using Moka.Server.Data;
using Moka.Server.Data.Results;
using Moka.Server.Models;
using MongoDB.Driver;

namespace Moka.Server.Service
{
    public class UserService
    {
        private readonly IMongoCollection<UserData> _users;

        public UserService(IMokaDataBaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<UserData>(settings.UsersCollectionName);
        }

        public async Task<CreateUserResult> FindOrCreate(User user)
        {
            var find = await Find(user);
            if (find == null)
            {
                user.Guid = Guid.NewGuid();
                await _users.InsertOneAsync(UserData.FromUser(user));
                var newUser = await Find(user);
                return new CreateUserResult(false,true,newUser.ToUser());
            }
            return new CreateUserResult(true,false,find.ToUser());
        }
        

        public async Task<UserData> Find(User user)
        {
            var result = await _users.FindAsync(u => u.Name == user.Name || u.Guid == user.Guid);
            return await result.FirstOrDefaultAsync();
        }
        
    }
}