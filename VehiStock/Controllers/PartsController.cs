using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Application.DTOs.Customer;
using Microsoft.EntityFrameworkCore;
using VehiStock.Application.DTOs;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Domain.Constants;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Staff}")]
    public class PartsController : ControllerBase
    {
        private readonly IPartRepository _partRepository;
        private readonly ApplicationDbContext _context;
        private readonly ICustomerPartRequestService _partRequestService;
        private readonly IImageStorageService _imageStorageService;

        public PartsController(IPartRepository partRepository, ApplicationDbContext context, ICustomerPartRequestService partRequestService, IImageStorageService imageStorageService)
        {
            _partRepository = partRepository;
            _context = context;
            _partRequestService = partRequestService;
            _imageStorageService = imageStorageService;
        }

        // GET /api/parts?search=brake&pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var all = await _partRepository.GetAllAsync();

            // Optional search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim();
                all = all.Where(p =>
                    p.PartName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    p.PartCode.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (p.Brand != null && p.Brand.Contains(q, StringComparison.OrdinalIgnoreCase)));
            }

            var totalRecords = all.Count();
            var paged = all
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PartDto
                {
                    PartId          = p.PartId,
                    PartCategoryId  = p.PartCategoryId,
                    CategoryName    = p.PartCategory != null ? p.PartCategory.Name : "Default",
                    PartCode        = p.PartCode,
                    PartName        = p.PartName,
                    Brand           = p.Brand,
                    PartPhotoUrl    = p.PartPhotoUrl,
                    UnitCost        = p.UnitCost,
                    UnitPrice       = p.UnitPrice,
                    StockQty        = p.StockQty,
                    MinimumStock    = p.MinimumStock,
                    IsActive        = p.IsActive
                })
                .ToList();

            var result = new VehiStock.Application.Dtos.Common.PaginatedResponse<PartDto>
            {
                Items        = paged,
                PageNumber   = pageNumber,
                PageSize     = pageSize,
                TotalRecords = totalRecords,
                TotalPages   = (int)Math.Ceiling(totalRecords / (double)pageSize)
            };

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var part = await _partRepository.GetByIdAsync(id);

            if (part == null)
                return NotFound();

            var result = new PartDto
            {
                PartId = part.PartId,
                PartCategoryId = part.PartCategoryId,
                CategoryName = part.PartCategory != null ? part.PartCategory.Name : "Default",
                PartCode = part.PartCode,
                PartName = part.PartName,
                Brand = part.Brand,
                PartPhotoUrl = part.PartPhotoUrl,
                UnitCost = part.UnitCost,
                UnitPrice = part.UnitPrice,
                StockQty = part.StockQty,
                MinimumStock = part.MinimumStock,
                IsActive = part.IsActive
            };

            return Ok(result);
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateFromJson([FromBody] CreatePartDtoJson dto)
        {
            var defaultCategory = await _context.PartCategories
                .FirstOrDefaultAsync(c => c.Name == "Default");

            if (defaultCategory == null)
            {
                defaultCategory = new PartCategory { Name = "Default" };
                _context.PartCategories.Add(defaultCategory);
                await _context.SaveChangesAsync();
            }

            var part = new Part
            {
                PartCategoryId = defaultCategory.PartCategoryId,
                PartCode = dto.PartCode,
                PartName = dto.PartName,
                Brand = dto.Brand ?? string.Empty,
                PartPhotoUrl = null,
                UnitCost = dto.UnitCost,
                UnitPrice = dto.UnitPrice,
                StockQty = dto.StockQty,
                MinimumStock = dto.MinimumStock,
                IsActive = true
            };

            await _partRepository.AddAsync(part);
            return Ok(new { message = "Part created successfully" });
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreatePartDto dto)
        {
            var defaultCategory = await _context.PartCategories
                .FirstOrDefaultAsync(c => c.Name == "Default");

            if (defaultCategory == null)
            {
                defaultCategory = new PartCategory
                {
                    Name = "Default",
                };

                _context.PartCategories.Add(defaultCategory);
                await _context.SaveChangesAsync();
            }

            string? photoUrl = null;
            if (dto.PartPhoto != null)
            {
                var uploadFile = MapImageUploadFile(dto.PartPhoto);
                if (uploadFile != null)
                {
                    photoUrl = await _imageStorageService.SaveImageAsync(uploadFile, "parts");
                }
            }

            var part = new Part
            {
                PartCategoryId = defaultCategory.PartCategoryId,
                PartCode = dto.PartCode,
                PartName = dto.PartName,
                Brand = dto.Brand ?? string.Empty,
                PartPhotoUrl = photoUrl,
                UnitCost = dto.UnitCost,
                UnitPrice = dto.UnitPrice,
                StockQty = dto.StockQty,
                MinimumStock = dto.MinimumStock,
                IsActive = true
            };

            await _partRepository.AddAsync(part);

            return Ok(new { message = "Part created successfully" });
        }

        [HttpPut]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateFromJson([FromBody] UpdatePartDtoJson dto)
        {
            var existing = await _partRepository.GetByIdAsync(dto.PartId);

            if (existing == null)
                return NotFound();

            existing.PartCode = dto.PartCode;
            existing.PartName = dto.PartName;
            existing.Brand = dto.Brand ?? string.Empty;
            existing.UnitCost = dto.UnitCost;
            existing.UnitPrice = dto.UnitPrice;
            existing.StockQty = dto.StockQty;
            existing.MinimumStock = dto.MinimumStock;
            existing.IsActive = dto.IsActive;

            await _partRepository.UpdateAsync(existing);
            return Ok(new { message = "Part updated successfully" });
        }

        [HttpPut]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update([FromForm] UpdatePartDto dto)
        {
            var existing = await _partRepository.GetByIdAsync(dto.PartId);

            if (existing == null)
                return NotFound();

            if (dto.RemovePartPhoto && !string.IsNullOrEmpty(existing.PartPhotoUrl))
            {
                _imageStorageService.DeleteImage(existing.PartPhotoUrl);
                existing.PartPhotoUrl = null;
            }

            if (dto.PartPhoto != null)
            {
                if (!string.IsNullOrEmpty(existing.PartPhotoUrl))
                {
                    _imageStorageService.DeleteImage(existing.PartPhotoUrl);
                }
                var uploadFile = MapImageUploadFile(dto.PartPhoto);
                if (uploadFile != null)
                {
                    existing.PartPhotoUrl = await _imageStorageService.SaveImageAsync(uploadFile, "parts");
                }
            }

            existing.PartCode = dto.PartCode;
            existing.PartName = dto.PartName;
            existing.Brand = dto.Brand ?? string.Empty;
            existing.UnitCost = dto.UnitCost;
            existing.UnitPrice = dto.UnitPrice;
            existing.StockQty = dto.StockQty;
            existing.MinimumStock = dto.MinimumStock;
            existing.IsActive = dto.IsActive;

            await _partRepository.UpdateAsync(existing);

            return Ok(new { message = "Part updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _partRepository.DeleteAsync(id);
            return Ok(new { message = "Part deleted successfully" });
        }

        // NEW: Create a part request for a specific part (used when stock is low)
        [HttpPost("{partId}/request")]
        public async Task<IActionResult> CreatePartRequest(int partId, [FromBody] CreatePartRequestDto request)
        {
            // Ensure the part exists
            var part = await _partRepository.GetByIdAsync(partId);
            if (part == null)
                return NotFound("Part not found.");

            var customerRequest = new VehiStock.Application.Dtos.Customer.CreatePartRequestRequest
            {
                RequestedPartName = part.PartName,
                Quantity = request.Quantity,
                Details = $"Low stock request for Part: {part.PartName} (Code: {part.PartCode}). Note: {request.Note}"
            };

            var result = await _partRequestService.CreatePartRequestAsync(GetCurrentUserId(), customerRequest, CancellationToken.None);
            return Ok(result);
        }

        private string GetCurrentUserId()
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return userId ?? string.Empty;
        }

        private static VehiStock.Application.Dtos.Common.ImageUploadFile? MapImageUploadFile(Microsoft.AspNetCore.Http.IFormFile? file)
        {
            if (file is null)
            {
                return null;
            }

            return new VehiStock.Application.Dtos.Common.ImageUploadFile(
                file.FileName,
                file.ContentType,
                file.Length,
                file.OpenReadStream,
                file.CopyToAsync);
        }
    }

    public class CreatePartDtoJson
    {
        public int PartCategoryId { get; set; }
        public string PartCode { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public decimal UnitCost { get; set; }
        public decimal UnitPrice { get; set; }
        public int StockQty { get; set; }
        public int MinimumStock { get; set; }
    }

    public class UpdatePartDtoJson
    {
        public int PartId { get; set; }
        public int PartCategoryId { get; set; }
        public string PartCode { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public decimal UnitCost { get; set; }
        public decimal UnitPrice { get; set; }
        public int StockQty { get; set; }
        public int MinimumStock { get; set; }
        public bool IsActive { get; set; }
    }
}

