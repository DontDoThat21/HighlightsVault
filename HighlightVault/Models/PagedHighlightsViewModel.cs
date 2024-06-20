namespace HighlightsVault.Models
{
    public class PagedHighlightsViewModel
    {
        public List<Highlight> Highlights { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }       
        public int TotalHighlights { get; set; }
    }

}
