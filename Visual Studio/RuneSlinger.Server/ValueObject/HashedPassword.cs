using System;
using System.Security.Cryptography;
using RuneSlinger.Base.Extensions;

namespace RuneSlinger.Server.ValueObject
{
    public class HashedPassword
    {
        public static HashedPassword FromPlainText(string plainTextPassword)
        {
            var random = new RNGCryptoServiceProvider();
            var bytes = new byte[16];
            random.GetBytes(bytes);
            var salt = bytes.ToHexString();

            return new HashedPassword((plainTextPassword + salt).ToSha1(), salt);
        }

        public string Hash { get; private set; }
        public string Salt { get; set; }

        public HashedPassword(string hash, string salt)
        {
            Hash = hash;
            Salt = salt;
        }

        /// <summary>
        /// For nHibernate
        /// </summary>
        private HashedPassword()
        {
            
        }

        public bool EqualsPlainText(string plainText)
        {
            return (plainText + Salt).ToSha1() == Hash;
        }
       
    }
}
