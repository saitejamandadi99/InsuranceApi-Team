namespace Insurance_Backend.DTO.ResponseDTO
{
    public class PaginatedResponseDTO<T>
    {
        public IEnumerable<T> Records { get; set; } = Enumerable.Empty<T>();

        //number of records per page
        public int PageSize { get; set; }
        //current page number
        public int CurrentPage { get; set; }

        //total records in the database
        public int TotalRecords { get; set; }

        //total number of pages
        public int TotalPages { get; set; }

        //indicates whether this is the last page
        public bool IsLastPage { get; set; }

        //direction = asc or desc 
        public string? SortDirection { get; set; }

        //optional sorting information
        public string? SortBy { get; set; }
    }
}
