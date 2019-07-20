using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulus.Aws.Common.Abstraction
{
    public interface IDateTimeService
    {
        DateTime GetCurrentServerTime();

        bool TryParseDateTime(string dateTimeStr,out DateTime dateTime);

        double GetTimeStamp(DateTime dateTime);
    }
}
