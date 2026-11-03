using Insurance_Backend.Data;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance_Backend.Repositories.classes
{
    public class CustomerProfileRepository : ICustomerProfileRepository
    {
        private readonly AppDbContext _context;

        public CustomerProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        // Add Customer Profile
        public async Task<CustomerProfile> AddCustomerProfile(
            CustomerProfile customerProfile)
        {
            DateTime currentDate = DateTime.UtcNow;

            customerProfile.CreatedDate = currentDate;
            customerProfile.UpdatedDate = currentDate;

            _context.CustomerProfile.Add(customerProfile);

            await _context.SaveChangesAsync();

            return await GetCustomerProfileById(customerProfile.CustomerId)
                ?? throw new InvalidOperationException(
                    "Customer profile was created but could not be retrieved.");
        }

        // Get Customer Profile By ID
        public async Task<CustomerProfile?> GetCustomerProfileById(int id)
        {
            return await _context.CustomerProfile
                .AsNoTracking()
                .Where(c => c.CustomerId == id)
                .Select(c => new CustomerProfile
                {
                    CustomerId = c.CustomerId,
                    UserId = c.UserId,
                    User = new User
                    {
                        UserId = c.User.UserId,
                        FullName = c.User.FullName,
                        Email = c.User.Email,
                        Role = c.User.Role,
                        ActiveStatus = c.User.ActiveStatus
                    },
                    DateOfBirth = c.DateOfBirth,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    Pincode = c.Pincode,
                    NomineeName = c.NomineeName,
                    NomineeRelationship = c.NomineeRelationship,
                    ProfileImageUrl = c.ProfileImageUrl,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate
                })
                .FirstOrDefaultAsync();
        }

        // Get Customer Profile By User ID
        public async Task<CustomerProfile?> GetCustomerProfileByUserId(
            int userId)
        {
            return await _context.CustomerProfile
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .Select(c => new CustomerProfile
                {
                    CustomerId = c.CustomerId,
                    UserId = c.UserId,
                    User = new User
                    {
                        UserId = c.User.UserId,
                        FullName = c.User.FullName,
                        Email = c.User.Email,
                        Role = c.User.Role,
                        ActiveStatus = c.User.ActiveStatus
                    },
                    DateOfBirth = c.DateOfBirth,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    Pincode = c.Pincode,
                    NomineeName = c.NomineeName,
                    NomineeRelationship = c.NomineeRelationship,
                    ProfileImageUrl = c.ProfileImageUrl,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate
                })
                .FirstOrDefaultAsync();
        }

        // Get All Customer Profiles With Search, Sorting, And Pagination
        public async Task<PaginatedResponseDTO<CustomerProfile>>
            GetAllCustomerProfiles(PaginationQueryDTO query)
        {
            int pageNumber = query.PageNumber;
            int pageSize = query.PageSize;

            IQueryable<CustomerProfile> profiles = _context.CustomerProfile
                .AsNoTracking();

            // Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string searchTerm = query.Search.Trim();

                profiles = profiles.Where(c =>
                    c.Address.Contains(searchTerm) ||
                    c.City.Contains(searchTerm) ||
                    c.State.Contains(searchTerm) ||
                    c.NomineeName.Contains(searchTerm));
            }

            // Sorting
            string sortBy = string.IsNullOrWhiteSpace(query.SortBy)
                ? "createddate"
                : query.SortBy.Trim().ToLowerInvariant();

            string sortDirection =
                string.IsNullOrWhiteSpace(query.SortDirection)
                    ? "desc"
                    : query.SortDirection.Trim().ToLowerInvariant();

            profiles = (sortBy, sortDirection) switch
            {
                ("city", "desc") =>
                    profiles.OrderByDescending(c => c.City)
                            .ThenBy(c => c.CustomerId),

                ("city", _) =>
                    profiles.OrderBy(c => c.City)
                            .ThenBy(c => c.CustomerId),

                ("state", "desc") =>
                    profiles.OrderByDescending(c => c.State)
                            .ThenBy(c => c.CustomerId),

                ("state", _) =>
                    profiles.OrderBy(c => c.State)
                            .ThenBy(c => c.CustomerId),

                ("createddate", "asc") =>
                    profiles.OrderBy(c => c.CreatedDate)
                            .ThenBy(c => c.CustomerId),

                ("createddate", _) =>
                    profiles.OrderByDescending(c => c.CreatedDate)
                            .ThenBy(c => c.CustomerId),

                _ =>
                    profiles.OrderByDescending(c => c.CreatedDate)
                            .ThenBy(c => c.CustomerId)
            };

            // Total records after filtering
            int totalRecords = await profiles.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            // Fetch paginated profiles
            var pagedProfiles = await profiles
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CustomerProfile
                {
                    CustomerId = c.CustomerId,
                    UserId = c.UserId,
                    DateOfBirth = c.DateOfBirth,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    Pincode = c.Pincode,
                    NomineeName = c.NomineeName,
                    NomineeRelationship = c.NomineeRelationship,
                    ProfileImageUrl = c.ProfileImageUrl,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate
                })
                .ToListAsync();

            return new PaginatedResponseDTO<CustomerProfile>
            {
                Records = pagedProfiles,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = pageNumber >= totalPages,
                SortBy = sortBy,
                SortDirection = sortDirection
            };
        }

        // Update Customer Profile
        public async Task<CustomerProfile?> UpdateCustomerProfile(
            CustomerProfile customerProfile)
        {
            var existingProfile = await _context.CustomerProfile
                .FirstOrDefaultAsync(c =>
                    c.CustomerId == customerProfile.CustomerId);

            if (existingProfile == null)
            {
                return null;
            }

            existingProfile.DateOfBirth = customerProfile.DateOfBirth;
            existingProfile.Address = customerProfile.Address;
            existingProfile.City = customerProfile.City;
            existingProfile.State = customerProfile.State;
            existingProfile.Pincode = customerProfile.Pincode;
            existingProfile.NomineeName = customerProfile.NomineeName;
            existingProfile.NomineeRelationship =
                customerProfile.NomineeRelationship;

            // Preserve the existing image when no new URL is provided.
            if (!string.IsNullOrWhiteSpace(customerProfile.ProfileImageUrl))
            {
                existingProfile.ProfileImageUrl =
                    customerProfile.ProfileImageUrl;
            }

            existingProfile.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetCustomerProfileById(
                customerProfile.CustomerId);
        }

        // Check Whether Profile Belongs To User
        public async Task<bool> IsProfileOwnedByUser(
            int customerId,
            int userId)
        {
            return await _context.CustomerProfile
                .AsNoTracking()
                .AnyAsync(c =>
                    c.CustomerId == customerId &&
                    c.UserId == userId);
        }

        // Check Whether User Already Has A Profile
        public async Task<bool> UserProfileExists(int userId)
        {
            return await _context.CustomerProfile
                .AsNoTracking()
                .AnyAsync(c => c.UserId == userId);
        }
    }
}
