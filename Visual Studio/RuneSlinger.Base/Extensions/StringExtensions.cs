using System.Security.Cryptography;
using System.Text;

namespace RuneSlinger.Base.Extensions
{
    public static class StringExtensions
    {
        public static string ToSha1(this string that)
        {
            return DoHash(that, new SHA1CryptoServiceProvider());
        }

        public static string ToMd5(this string that)
        {
            return DoHash(that, new MD5CryptoServiceProvider());
        }

        public static string DoHash(string that, HashAlgorithm hasher)
        {
            return hasher.ComputeHash(Encoding.UTF8.GetBytes(that)).ToHexString();
        }
    }
}
