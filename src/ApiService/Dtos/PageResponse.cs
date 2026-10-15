namespace ApiService.Dtos;

public record PageResponse<T>(List<T> Content, int Page, int PageSize, long TotalElements)
{
    public static PageResponse<T> Of(List<T> content, long totalElements, int page, int pageSize) =>
        new(content, page, pageSize, totalElements);
}
