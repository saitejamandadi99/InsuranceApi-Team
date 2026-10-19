using System.ComponentModel.DataAnnotations;

namespace Insurance_Backend.DTO.RequestDTO
{
    public class PaginationQueryDTO
    {
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; }

        public string? SortDirection { get; set; } = "asc";

        public string? Search { get; set; }
    }
}
