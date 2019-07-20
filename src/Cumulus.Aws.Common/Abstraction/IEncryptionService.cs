using Cumulus.Aws.Common.BusinessModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulus.Aws.Common.Abstraction
{
    public interface IEncryptionService
    {
        byte[] GetHashBytes(string input);

        string GetBucketObjectHash(StockMarket sourceBucketType, string objectKey);
    }
}
