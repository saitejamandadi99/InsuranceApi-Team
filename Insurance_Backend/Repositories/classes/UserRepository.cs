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
        public async Task<UserResponseDTO?> ActivateUser(int id)
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
                .Select(u => new UserResponseDTO
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    MobileNumber = u.MobileNumber,
                    Role = u.Role,
                    ActiveStatus = u.ActiveStatus,
                    CreatedDate = u.CreatedDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UserResponseDTO> AddUser(User user)
        {
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return await _context.User
                .AsNoTracking()
                .Where(u => u.UserId == user.UserId)
                .Select(u => new UserResponseDTO
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    MobileNumber = u.MobileNumber,
                    Role = u.Role,
                    ActiveStatus = u.ActiveStatus,
                    CreatedDate = u.CreatedDate
                })
                .FirstAsync();
        }

        public async Task<UserResponseDTO?> DeactivateUser(int id)
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
                .Select(u => new UserResponseDTO
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    MobileNumber = u.MobileNumber,
                    Role = u.Role,
                    ActiveStatus = u.ActiveStatus,
                    CreatedDate = u.CreatedDate
                })
                .FirstOrDefaultAsync();
        }

        public Task<bool> EmailExists(string email)
        {
            return  _context.User.AnyAsync(u => u.Email == email); //anyasync checks the existing of the email in the database and returns true or false
        }

        public async Task<PaginatedResponseDTO<UserResponseDTO>> GetAllUsers(PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1); // to ensure that the page number is at least 1
            int pageSize = Math.Clamp(query.PageSize, 1, 100); // to ensure that the page size is between 1 and 100 if the page size is less than 1 it will be set to 1 and if it is greater than 100 it will be set to 100

            IQueryable<User> users = _context.User.AsNoTracking();

            //search 
            if(!string.IsNullOrWhiteSpace(query.Search))
            {
                string searchTerm = query.Search.Trim().ToLower();
                users = users.Where(u => u.FullName.Contains(searchTerm) || u.Email.Contains(searchTerm));
            }

            //sorting 

            users = (query.SortBy?.ToLowerInvariant(), query.SortDirection?.ToLowerInvariant()) switch
            {
                ("fullname","desc") =>users.OrderByDescending(u=>u.FullName).ThenBy(u=>u.UserId),
                ("fullname",_) =>users.OrderBy(u=>u.FullName).ThenBy(u=>u.UserId),
                ("email", "desc") => users.OrderByDescending(u => u.Email).ThenBy(u => u.UserId),
                ("email", _) => users.OrderBy(u => u.Email).ThenBy(u => u.UserId),
                ("createddate", "desc") => users.OrderByDescending(u => u.CreatedDate).ThenBy(u => u.UserId),
                ("createddate", _) => users.OrderBy(u => u.CreatedDate).ThenBy(u => u.UserId),

                _ => users.OrderBy(u => u.UserId)
            };

            int totalRecords = await users.CountAsync();

            int totalPages  = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var pagedUsers = await users.Select(u => new UserResponseDTO
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                MobileNumber = u.MobileNumber,
                Role = u.Role,
                ActiveStatus = u.ActiveStatus,
                CreatedDate = u.CreatedDate
            }).Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();


            return new PaginatedResponseDTO<UserResponseDTO>
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
            return await _context.User.FirstOrDefaultAsync(user=>user.Email == email);  //returns first user with the given email or null if not found
        }

        public async Task<User?> GetUserById(int id)
        {
            return await _context.User.FindAsync(id); //uses the primary key to find the user with the given id or null if not found , better to use FindAsync when searching by primary key as it is more efficient than FirstOrDefaultAsync
        }
    }
}
