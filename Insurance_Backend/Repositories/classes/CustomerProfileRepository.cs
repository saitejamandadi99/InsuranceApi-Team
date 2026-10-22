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
            _context.CustomerProfile.Add(customerProfile);

            await _context.SaveChangesAsync();

            return await _context.CustomerProfile
                .AsNoTracking()
                .Where(c => c.CustomerId == customerProfile.CustomerId)
                .Select(c => new CustomerProfile
                {
                    CustomerId = c.CustomerId,
                    DateOfBirth = c.DateOfBirth,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    Pincode = c.Pincode,
                    NomineeName = c.NomineeName,
                    NomineeRelationship = c.NomineeRelationship,
                    ProfileImageUrl = c.ProfileImageUrl
                })
                .FirstAsync();
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
                    DateOfBirth = c.DateOfBirth,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    Pincode = c.Pincode,
                    NomineeName = c.NomineeName,
                    NomineeRelationship = c.NomineeRelationship,
                    ProfileImageUrl = c.ProfileImageUrl
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
                    DateOfBirth = c.DateOfBirth,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    Pincode = c.Pincode,
                    NomineeName = c.NomineeName,
                    NomineeRelationship = c.NomineeRelationship,
                    ProfileImageUrl = c.ProfileImageUrl
                })
                .FirstOrDefaultAsync();
        }

        // Get All Customer Profiles With Search, Sorting, And Pagination
        public async Task<PaginatedResponseDTO<CustomerProfile>>
            GetAllCustomerProfiles(PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            IQueryable<CustomerProfile> profiles = _context.CustomerProfile
                .AsNoTracking();

            // Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string searchTerm = query.Search.Trim().ToLower();

                profiles = profiles.Where(c =>
                    c.Address.ToLower().Contains(searchTerm) ||
                    c.City.ToLower().Contains(searchTerm) ||
                    c.State.ToLower().Contains(searchTerm) ||
                    c.NomineeName.ToLower().Contains(searchTerm));
            }

            // Sorting
            profiles = (
                query.SortBy?.ToLowerInvariant(),
                query.SortDirection?.ToLowerInvariant()
            ) switch
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

                _ => profiles.OrderBy(c => c.CustomerId)
            };

            int totalRecords = await profiles.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            var pagedProfiles = await profiles
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CustomerProfile
                {
                    CustomerId = c.CustomerId,
                    DateOfBirth = c.DateOfBirth,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    Pincode = c.Pincode,
                    NomineeName = c.NomineeName,
                    NomineeRelationship = c.NomineeRelationship,
                    ProfileImageUrl = c.ProfileImageUrl
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
                SortBy = query.SortBy,
                SortDirection = query.SortDirection
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

            return await _context.CustomerProfile
                .AsNoTracking()
                .Where(c => c.CustomerId == customerProfile.CustomerId)
                .Select(c => new CustomerProfile
                {
                    CustomerId = c.CustomerId,
                    DateOfBirth = c.DateOfBirth,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    Pincode = c.Pincode,
                    NomineeName = c.NomineeName,
                    NomineeRelationship = c.NomineeRelationship,
                    ProfileImageUrl = c.ProfileImageUrl
                })
                .FirstOrDefaultAsync();
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

        // Delete Customer Profile
        public async Task<bool> DeleteCustomerProfile(int id)
        {
            var profile = await _context.CustomerProfile
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (profile == null)
            {
                return false;
            }

            _context.CustomerProfile.Remove(profile);

            await _context.SaveChangesAsync();

            return true;
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
