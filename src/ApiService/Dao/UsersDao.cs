using ApiService.Data;
using ApiService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Dao;

public record UsersFilter(
    string? Username = null,
    string? Email = null,
    string? FullName = null,
    string? Role = null,
    bool? MfaEnabled = null,
    bool? IsActive = null,
    DateTimeOffset? CreatedAt = null,
    DateTimeOffset? LastLoginAt = null,
    string? MfaSecret = null,
    DateTimeOffset? MfaEnrolledAt = null);

public class UsersDao(AppDbContext db)
{
    public Task<Users?> FindAsync(Guid id) => db.Users.FindAsync(id).AsTask();

    public Task<Users?> GetByEmailAsync(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Task.FromResult<Users?>(null);
        }

        var normalized = Paging.Term(email);
        return db.Users.AsNoTracking().FirstOrDefaultAsync(e => e.Email.ToLower() == normalized);
    }

    public async Task<Guid> DeleteAsync(Guid id)
    {
        await db.Users.Where(e => e.Id == id).ExecuteDeleteAsync();
        return id;
    }

    public async Task<Users> CreateAsync(Users e)
    {
        db.Users.Add(e);
        await db.SaveChangesAsync();
        return e;
    }

    public async Task<Users> UpdateAsync(Users e)
    {
        db.Users.Update(e);
        await db.SaveChangesAsync();
        return e;
    }

    public Task<List<Users>> SelectAllAsync(int page, int pageSize, UsersFilter filter) =>
        BuildDynamicQuery(filter).OrderBy(e => e.Id).ApplyPagination(page, pageSize).ToListAsync();

    public Task<long> CountAllAsync(UsersFilter filter) => BuildDynamicQuery(filter).LongCountAsync();

    private IQueryable<Users> BuildDynamicQuery(UsersFilter f)
    {
        var q = db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(f.Username))
        {
            var t = Paging.Term(f.Username);
            q = q.Where(e => e.Username.ToLower().Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(f.Email))
        {
            var t = Paging.Term(f.Email);
            q = q.Where(e => e.Email.ToLower().Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(f.FullName))
        {
            var t = Paging.Term(f.FullName);
            q = q.Where(e => e.FullName.ToLower().Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(f.Role))
        {
            var t = Paging.Term(f.Role);
            q = q.Where(e => e.Role.ToLower().Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(f.MfaSecret))
        {
            var t = Paging.Term(f.MfaSecret);
            q = q.Where(e => e.MfaSecret!.ToLower().Contains(t));
        }

        if (f.MfaEnabled is not null)
        {
            q = q.Where(e => e.MfaEnabled == f.MfaEnabled);
        }

        if (f.IsActive is not null)
        {
            q = q.Where(e => e.IsActive == f.IsActive);
        }

        if (f.CreatedAt is not null)
        {
            q = q.Where(e => e.CreatedAt == f.CreatedAt);
        }

        if (f.LastLoginAt is not null)
        {
            q = q.Where(e => e.LastLoginAt == f.LastLoginAt);
        }

        if (f.MfaEnrolledAt is not null)
        {
            q = q.Where(e => e.MfaEnrolledAt == f.MfaEnrolledAt);
        }

        return q;
    }
}
