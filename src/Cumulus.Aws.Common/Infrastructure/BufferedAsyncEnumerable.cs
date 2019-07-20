#region - References -
using Cumulus.Aws.Common.Abstraction;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; 
#endregion

namespace Cumulus.Aws.Common.Infrastructure
{
    public class BufferedAsyncEnumerable<T> : IAsyncEnumerable<ICollection<T>>, IAsyncEnumerator<ICollection<T>>
    {
        #region - Private Properties -

        private IAsyncEnumerator<T> innerAsyncEnumerator;

        private int size;

        #endregion

        #region - Constructor -

        public BufferedAsyncEnumerable(IAsyncEnumerable<T> asyncEnumerable, int size)
        {
            this.innerAsyncEnumerator = asyncEnumerable.GetEnumerator();
            this.size = size;
        }

        #endregion

        #region - Public Methods -

        public ICollection<T> Current { get; private set; }

        object IAsyncEnumerator.Current => this.Current;

        public void Dispose()
        {
            this.innerAsyncEnumerator.Dispose();
        }

        public IAsyncEnumerator<ICollection<T>> GetEnumerator()
        {
            return this;
        }

        public async Task<bool> MoveNextAsync()
        {
            this.Current = new List<T>();
            var current = 0;

            while (await this.innerAsyncEnumerator.MoveNextAsync() && current++ < size)
                this.Current.Add(this.innerAsyncEnumerator.Current);

            return this.Current.Any();
        }

        public void Reset()
        {
            this.innerAsyncEnumerator.Reset();
        }

        IAsyncEnumerator IAsyncEnumerable.GetEnumerator()
        {
            return this;
        }

        #endregion
    }
}