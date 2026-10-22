
using Insurance_Backend.Data;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance_Backend.Repositories.classes
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<InsuranceProduct?> ActivateProduct(int productId)
        {
            var product = await _context.InsuranceProduct
                .FindAsync(productId);

            if (product == null)
            {
                return null;
            }

            product.ActiveStatus = ActiveStatus.Active;
            product.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await _context.InsuranceProduct
                .AsNoTracking()
                .Where(p => p.ProductId == productId)
                .Select(p => new InsuranceProduct
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductType = p.ProductType,
                    Description = p.Description,
                    ActiveStatus = p.ActiveStatus,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<InsuranceProduct> CreateProduct(
            InsuranceProduct product)
        {
            _context.InsuranceProduct.Add(product);

            await _context.SaveChangesAsync();

            return await _context.InsuranceProduct
                .AsNoTracking()
                .Where(p => p.ProductId == product.ProductId)
                .Select(p => new InsuranceProduct
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductType = p.ProductType,
                    Description = p.Description,
                    ActiveStatus = p.ActiveStatus,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate
                })
                .FirstAsync();
        }

        public async Task<InsuranceProduct?> DeactivateProduct(int productId)
        {
            var product = await _context.InsuranceProduct
                .FindAsync(productId);

            if (product == null)
            {
                return null;
            }

            product.ActiveStatus = ActiveStatus.Inactive;
            product.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await _context.InsuranceProduct
                .AsNoTracking()
                .Where(p => p.ProductId == productId)
                .Select(p => new InsuranceProduct
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductType = p.ProductType,
                    Description = p.Description,
                    ActiveStatus = p.ActiveStatus,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<InsuranceProduct?> GetProductById(int productId)
        {
            return await _context.InsuranceProduct
                .AsNoTracking()
                .Where(p => p.ProductId == productId)
                .Select(p => new InsuranceProduct
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductType = p.ProductType,
                    Description = p.Description,
                    ActiveStatus = p.ActiveStatus,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<InsuranceProduct?> GetProductByProductName(
            string productName)
        {
            return await _context.InsuranceProduct
                .AsNoTracking()
                .Where(p => p.ProductName == productName)
                .Select(p => new InsuranceProduct
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductType = p.ProductType,
                    Description = p.Description,
                    ActiveStatus = p.ActiveStatus,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PaginatedResponseDTO<InsuranceProduct>> ListProducts(
            PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            IQueryable<InsuranceProduct> products =
                _context.InsuranceProduct.AsNoTracking();

            // Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string searchTerm = query.Search.Trim().ToLower();

                products = products.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm));
            }

            // Sorting
            products = (
                query.SortBy?.ToLowerInvariant(),
                query.SortDirection?.ToLowerInvariant()
            ) switch
            {
                ("productname", "desc") =>
                    products.OrderByDescending(p => p.ProductName)
                            .ThenBy(p => p.ProductId),

                ("productname", _) =>
                    products.OrderBy(p => p.ProductName)
                            .ThenBy(p => p.ProductId),

                ("description", "desc") =>
                    products.OrderByDescending(p => p.Description)
                            .ThenBy(p => p.ProductId),

                ("description", _) =>
                    products.OrderBy(p => p.Description)
                            .ThenBy(p => p.ProductId),

                _ => products.OrderBy(p => p.ProductId)
            };

            int totalRecords = await products.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            var pagedProducts = await products
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new InsuranceProduct
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductType = p.ProductType,
                    Description = p.Description,
                    ActiveStatus = p.ActiveStatus,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate
                })
                .ToListAsync();

            return new PaginatedResponseDTO<InsuranceProduct>
            {
                Records = pagedProducts,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = pageNumber >= totalPages,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection
            };
        }

        public async Task<InsuranceProduct?> UpdateProduct(
            int productId,
            InsuranceProduct product)
        {
            var existingProduct = await _context.InsuranceProduct
                .FindAsync(productId);

            if (existingProduct == null)
            {
                return null;
            }

            existingProduct.ProductName = product.ProductName;
            existingProduct.ProductType = product.ProductType;
            existingProduct.Description = product.Description;
            existingProduct.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await _context.InsuranceProduct
                .AsNoTracking()
                .Where(p => p.ProductId == productId)
                .Select(p => new InsuranceProduct
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductType = p.ProductType,
                    Description = p.Description,
                    ActiveStatus = p.ActiveStatus,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate
                })
                .FirstOrDefaultAsync();
        }
    }
}
