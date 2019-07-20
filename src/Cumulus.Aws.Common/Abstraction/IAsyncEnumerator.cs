using System;
using System.Threading.Tasks;

namespace Cumulus.Aws.Common.Abstraction
{
    public interface IAsyncEnumerator
    {
        Task<bool> MoveNextAsync();

        Object Current
        {
            get;
        }

        void Reset();
    }

    public interface IAsyncEnumerator<out T> : IDisposable, IAsyncEnumerator
    {
        new T Current
        {
            get;
        }
    }
}
