using Cumulus.Aws.Common.Abstraction;
using System;

namespace Cumulus.Aws.Common.Implementation
{
    internal class DateTimeService : IDateTimeService
    {
        #region - Private Properties -

        private DateTime epochDateTime;

        #endregion

        #region - Constructor -

        public DateTimeService()
        {
            this.epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        #endregion

        #region - Public Methods | IDateTimeService -

        public DateTime GetCurrentServerTime()
        {
            return DateTime.UtcNow;
        }

        public double GetTimeStamp(DateTime dateTime)
        {
            return (dateTime - this.epochDateTime).TotalMilliseconds;
        }

        public bool TryParseDateTime(string dateTimeStr, out DateTime dateTime)
        {
            return DateTime.TryParse(dateTimeStr, out dateTime);
        }

        #endregion
    }
}
