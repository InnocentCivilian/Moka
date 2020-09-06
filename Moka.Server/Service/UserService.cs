using System;
using System.Threading.Tasks;
using Moka.Server.Data;
using Moka.Server.Data.Results;
using Moka.Server.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Moka.Server.Service
{
    public interface IUserService
    {
        Task<CreateUserResult> FindOrCreate(UserModel user);
        Task<UserData> FindAsync(UserModel user);
        Task<UserData> FindAsync(string name);
        UserData Find(string name);
        UserData FindById(string id);
        Task Update(UserData user);
    }

    public class UserService : IUserService
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
            var result = await _users.FindAsync(u => u.Username == user.UserName || u.Guid == user.Guid);
            return await result.FirstOrDefaultAsync();
        }

        public async Task<UserData> FindAsync(string name)
        {
            var result = await _users.FindAsync(u => u.Username == name);
            return await result.FirstOrDefaultAsync();
        }

        public UserData Find(string name)
        {
            var result = _users.Find(u => u.Username == name);
            var user = result.First();
            return user;
        }

        public UserData FindById(string id)
        {
            var result = _users.Find(u => u.Id == id);
            var user = result.First();
            return user;
        }

        public async Task Update(UserData user)
        {
            var filter = Builders<UserData>.Filter.Eq(u => u.Guid, user.Guid);
            await _users.ReplaceOneAsync(filter, user);
        }
    }
}