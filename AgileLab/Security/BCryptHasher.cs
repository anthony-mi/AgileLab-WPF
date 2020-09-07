using System;
using System.Runtime.InteropServices;
using System.Security;

namespace AgileLab.Security
{
    class BCryptHasher : IHasher
    {
        private string _salt = Properties.Settings.Default.HasherSalt;

        public string CalculateHash(SecureString inputKey)
        {
            IntPtr stringPointer = Marshal.SecureStringToBSTR(inputKey);
            string str = string.Empty;

            try
            {
                str = Marshal.PtrToStringBSTR(stringPointer);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(stringPointer);
            }

            int workFactor = Properties.Settings.Default.hasherWorkFactor;
            return BCrypt.Net.BCrypt.HashPassword(str, Properties.Settings.Default.HasherSalt);
        }
    }
}
