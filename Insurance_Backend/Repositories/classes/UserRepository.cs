
using Insurance_Backend.Data;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance_Backend.Repositories.classes
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> ActivateUser(int id)
        {
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return null;
            }

            user.ActiveStatus = ActiveStatus.Active;
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await _context.User
                .AsNoTracking()
                .Where(u => u.UserId == id)
                .Select(u => new User
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    MobileNumber = u.MobileNumber,
                    Role = u.Role,
                    ActiveStatus = u.ActiveStatus,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<User> AddUser(User user)
        {
            _context.User.Add(user);

            await _context.SaveChangesAsync();

            return await _context.User
                .AsNoTracking()
                .Where(u => u.UserId == user.UserId)
                .Select(u => new User
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    MobileNumber = u.MobileNumber,
                    Role = u.Role,
                    ActiveStatus = u.ActiveStatus,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate
                })
                .FirstAsync();
        }

        public async Task<User?> DeactivateUser(int id)
        {
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return null;
            }

            user.ActiveStatus = ActiveStatus.Inactive;
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await _context.User
                .AsNoTracking()
                .Where(u => u.UserId == id)
                .Select(u => new User
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    MobileNumber = u.MobileNumber,
                    Role = u.Role,
                    ActiveStatus = u.ActiveStatus,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate
                })
                .FirstOrDefaultAsync();
        }

        public Task<bool> EmailExists(string email)
        {
            return _context.User
                .AnyAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByVerificationCodeHash(string verificationCodeHash)
        {
            return await _context.User.FirstOrDefaultAsync(user =>

            user.EmailVerificationCodeHash == verificationCodeHash);

        }
        public async Task<User> UpdateUser(User user)
        {
            user.UpdatedDate = DateTime.UtcNow;


            _context.User.Update(user);


            await _context.SaveChangesAsync();
            return user;

        }


        public async Task<PaginatedResponseDTO<User>> GetAllUsers(
            PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            IQueryable<User> users = _context.User
                .AsNoTracking();

            // Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string searchTerm = query.Search.Trim().ToLower();
                var roleMatches = Enum.GetValues<Role>()
                .Where(r => r.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

                var statusMatches = Enum.GetValues<ActiveStatus>()
                    .Where(s => s.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

                users = users.Where(u =>
                    u.FullName.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm) ||
                    roleMatches.Contains(u.Role) ||    
                    statusMatches.Contains(u.ActiveStatus));
            }

            // Sorting
            users = (
                query.SortBy?.ToLowerInvariant(),
                query.SortDirection?.ToLowerInvariant()
            ) switch
            {
                ("fullname", "desc") =>
                    users.OrderByDescending(u => u.FullName)
                         .ThenBy(u => u.UserId),

                ("fullname", _) =>
                    users.OrderBy(u => u.FullName)
                         .ThenBy(u => u.UserId),

                ("email", "desc") =>
                    users.OrderByDescending(u => u.Email)
                         .ThenBy(u => u.UserId),

                ("email", _) =>
                    users.OrderBy(u => u.Email)
                         .ThenBy(u => u.UserId),

                ("createddate", "desc") =>
                    users.OrderByDescending(u => u.CreatedDate)
                         .ThenBy(u => u.UserId),

                ("createddate", _) =>
                    users.OrderBy(u => u.CreatedDate)
                         .ThenBy(u => u.UserId),

                _ => users.OrderBy(u => u.UserId)
            };

            int totalRecords = await users.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            var pagedUsers = await users
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new User
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    MobileNumber = u.MobileNumber,
                    Role = u.Role,
                    ActiveStatus = u.ActiveStatus,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate
                })
                .ToListAsync();

            return new PaginatedResponseDTO<User>
            {
                Records = pagedUsers,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = pageNumber >= totalPages,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection
            };
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            // Returns the full entity for authentication,
            // including the stored password hash.
            return await _context.User
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserById(int id)
        {
            return await _context.User
                .AsNoTracking()
                .Where(u => u.UserId == id)
                .Select(u => new User
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    MobileNumber = u.MobileNumber,
                    Role = u.Role,
                    ActiveStatus = u.ActiveStatus,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate
                })
                .FirstOrDefaultAsync();
        }
    }
}
