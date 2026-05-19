using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Management;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

// Implementation for admin staff management data access
public class StaffManagementRepository : IStaffManagementRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public StaffManagementRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyCollection<ApplicationUser> Users, int TotalRecords)> GetStaffUsersAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users
            .Include(x => x.StaffProfile)
            .Where(x => x.StaffProfile != null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(x =>
                x.FullName.ToLower().Contains(normalizedSearch) ||
                (x.Email != null && x.Email.ToLower().Contains(normalizedSearch)) ||
                (x.PhoneNumber != null && x.PhoneNumber.Contains(search.Trim())) ||
                (x.StaffProfile != null && x.StaffProfile.JobTitle.ToLower().Contains(normalizedSearch)) ||
                (x.StaffProfile != null && x.StaffProfile.StaffMemberId.ToString().Contains(search.Trim())));
        }

        query = query.OrderBy(x => x.FullName);

        var totalRecords = await query.CountAsync(cancellationToken);
        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (users, totalRecords);
    }

    public Task<ApplicationUser?> GetUserWithStaffProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _userManager.Users
            .Include(x => x.StaffProfile)
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetRolesAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToArray();
    }

    public async Task UpdateRoleAsync(ApplicationUser user, string role, CancellationToken cancellationToken = default)
    {
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(" ", removeResult.Errors.Select(x => x.Description)));
            }
        }

        var addResult = await _userManager.AddToRoleAsync(user, role);
        if (!addResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", addResult.Errors.Select(x => x.Description)));
        }
    }

    public async Task<IReadOnlyCollection<StaffInvoiceActivityResponse>> GetRecentInvoiceActivityAsync(int staffMemberId, int take, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SalesInvoices
            .AsNoTracking()
            .Where(x => x.StaffMemberId == staffMemberId)
            .OrderByDescending(x => x.InvoiceDate)
            .ThenByDescending(x => x.SalesInvoiceId)
            .Take(Math.Max(1, take))
            .Select(x => new StaffInvoiceActivityResponse
            {
                SalesInvoiceId = x.SalesInvoiceId,
                InvoiceNo = x.InvoiceNo,
                CustomerName = x.Customer.User.FullName,
                InvoiceDate = x.InvoiceDate,
                TotalAmount = x.TotalAmount,
                PaymentStatus = x.PaymentStatus
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<(int TotalInvoicesCreated, decimal TotalInvoiceValue)> GetInvoiceSummaryAsync(int staffMemberId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SalesInvoices
            .AsNoTracking()
            .Where(x => x.StaffMemberId == staffMemberId);

        var totalInvoicesCreated = await query.CountAsync(cancellationToken);
        var totalInvoiceValue = await query.SumAsync(x => (decimal?)x.TotalAmount, cancellationToken) ?? 0m;
        return (totalInvoicesCreated, totalInvoiceValue);
    }
}
