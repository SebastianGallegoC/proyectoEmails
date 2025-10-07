namespace Application.DTOs
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

        private int _total;
        public int Total
        {
            get => _total;
            set => _total = value < 0 ? 0 : value;
        }

        private int _page = 1;
        public int Page
        {
            get => _page;
            set => _page = value <= 0 ? 1 : value;
        }

        private int _pageSize = 20;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value <= 0 ? 20 : value;
        }
    }
}
