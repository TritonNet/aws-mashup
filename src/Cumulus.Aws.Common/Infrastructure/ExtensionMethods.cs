#region - References -
using Cumulus.Aws.Common.Abstraction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#endregion

namespace Cumulus.Aws.Common.Infrastructure
{
    public static class ExtensionMethods
    {
        #region - Extension Methods | IAsyncEnumerable<T> -

        public static async Task<ICollection<T>> Take<T>(this IAsyncEnumerable<T> asyncEnumerable, int size)
        {
            var bufferedAsyncEnumerable = new BufferedAsyncEnumerable<T>(asyncEnumerable, size);

            await bufferedAsyncEnumerable.MoveNextAsync();

            return bufferedAsyncEnumerable.Current;
        }

        #endregion

        #region - Extension Methods | Guid -

        public static string ToStringID(this Guid guid)
        {
            if (guid == Guid.Empty)
                return string.Empty;

            return guid.ToString("N");
        }

        #endregion

        #region - Extension Methods | Type -

        public static bool IsNumericType(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }
}