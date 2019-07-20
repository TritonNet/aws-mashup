using System.IO;

namespace Cumulus.Aws.Common.Abstraction
{
    public interface ICsvConverter
    {
        IAsyncEnumerable<T> Convert<T>(Stream csvStream, char seperator) where T : new();
    }
}
