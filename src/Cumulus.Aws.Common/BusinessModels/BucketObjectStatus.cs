using System;

namespace Cumulus.Aws.Common.BusinessModels
{
    public enum BucketObjectStatus
    {
        Unknown = 0,

        Detected = 1,

        Queued = 2,

        Processing = 3,

        Processed = 4
    }
}
