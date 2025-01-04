// https://github.com/villainoustourist/Blazor.Pagination/tree/master

namespace EBikeLibrary.Paginator;

public class PagedResult<T> : PagedResultBase where T : class
{
    public T[] Results { get; set; }
}