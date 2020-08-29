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

        public async Task<CreateUserResult> FindOrCreate(UserModel user)
        {
            var find = await FindAsync(user);
            if (find != null) return new CreateUserResult(true, false, find.ToUserModel());
            user.Guid = Guid.NewGuid();
            await _users.InsertOneAsync(UserData.FromUser(user));
            var newUser = await FindAsync(user);
            return new CreateUserResult(false, true, newUser.ToUserModel());
        }


        public async Task<UserData> FindAsync(UserModel user)
        {
            var result = await _users.FindAsync(u => u.Name == user.Name || u.Guid == user.Guid);
            return await result.FirstOrDefaultAsync();
        }

        public UserData Find(string name)
        {
            var result = _users.Find(u => u.Name == name);
            var user = result.First();
            return user;
        }
    }
}