using System.Collections.Generic;

namespace Cumulus.Aws.Common.BusinessModels
{
    public class PendingBucketObjectKeyCollection : List<BucketObject>
    {
        public bool IsTruncated { get; set; }

        public string LastMarker { get; set; }
    }
}
