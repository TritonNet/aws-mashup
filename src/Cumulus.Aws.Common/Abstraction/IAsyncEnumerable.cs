namespace Cumulus.Aws.Common.Abstraction
{
    public interface IAsyncEnumerable
    {
        IAsyncEnumerator GetEnumerator();
    }

    public interface IAsyncEnumerable<T> : IAsyncEnumerable
    {
        new IAsyncEnumerator<T> GetEnumerator();
    }
}
