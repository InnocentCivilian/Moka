using Moka.Server.Models;

namespace Moka.Server.Data.Results
{
    public class CreateUserResult
    {
        public bool IsAlreadyExists { get; set; }
        public bool IsCreated { get; set; }
        public UserModel UserModel { get; set; }

        public CreateUserResult(bool isAlreadyExists, bool isCreated, UserModel userModel)
        {
            IsAlreadyExists = isAlreadyExists;
            IsCreated = isCreated;
            UserModel = userModel;
        }
    }
} 