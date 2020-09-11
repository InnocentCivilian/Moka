namespace Moka.SharedKernel.Security
{
    public class Password
    {
        public string Passphrase { private get; set; }

        public Password(string passphrase)
        {
            Passphrase = passphrase;
        }

        public bool Equals(Password other)
        {
            return Passphrase.Equals(other.Passphrase);
        }
    }
}