using System.Security;

namespace AgileLab.Security
{
    interface IHasher
    {
        string CalculateHash(SecureString password);
    }
}
