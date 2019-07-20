#region - References -
using Cumulus.Aws.Common.Abstraction;
using Cumulus.Aws.Common.BusinessModels;
using Cumulus.Aws.Common.Infrastructure;
using System;
using System.Security.Cryptography;
using System.Text;
#endregion

namespace Cumulus.Aws.Common.Implementation
{
    internal class EncryptionService : IEncryptionService
    {
        #region - Public Methods | IEncryptionService -

        public string GetBucketObjectHash(StockMarket sourceBucketType, string objectKey)
        {
            var objectKeyMeta = $"{sourceBucketType}_{objectKey}";

            return new Guid(this.GetHashBytes(objectKeyMeta)).ToStringID();
        }

        public byte[] GetHashBytes(string input)
        {
            using (var x = new MD5CryptoServiceProvider())
            {
                var bs = Encoding.UTF8.GetBytes(input);
                return x.ComputeHash(bs);
            }
        }

        #endregion
    }
}
