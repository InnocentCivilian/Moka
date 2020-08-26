using Moka.Server.Models;

namespace Moka.Server.Data.Results
{
    public class CreateUserResult
    {
        public bool IsAlreadyExists { get; set; }
        public bool IsCreated { get; set; }
        public User User { get; set; }

        public CreateUserResult(bool isAlreadyExists, bool isCreated, User user)
        {
            IsAlreadyExists = isAlreadyExists;
            IsCreated = isCreated;
            User = user;
        }
    }
} 