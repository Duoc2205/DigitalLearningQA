using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Domain.Entities;
using DigitalLearningQA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy toàn bộ người dùng.
    /// Dùng cho màn hình quản lý người dùng của Admin.
    /// </summary>
    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Department)
            .OrderBy(x => x.FullName)
            .ToListAsync();
    }

    /// <summary>
    /// Phục vụ đăng nhập.
    /// Chỉ đọc dữ liệu nên dùng AsNoTracking để tăng hiệu năng.
    /// </summary>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Username == username && !x.IsDeleted);
    }

    /// <summary>
    /// Lấy User để cập nhật thông tin.
    /// Không dùng AsNoTracking để EF Core theo dõi thay đổi.
    /// </summary>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(x => x.Role)
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}
