namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record GithubUserProfileDto(
    string Login,
    long Id,
    string NodeId,
    string AvatarUrl,
    string HtmlUrl,
    string? Name,
    string? Company,
    string? Blog,
    string? Location,
    string? Email,
    string? Bio,
    int PublicRepos,
    int Followers,
    int Following,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
