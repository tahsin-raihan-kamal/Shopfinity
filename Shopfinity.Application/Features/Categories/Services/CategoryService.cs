using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Shopfinity.Application.Common.Interfaces;
using Shopfinity.Application.Features.Categories.DTOs;
using Shopfinity.Domain.Entities;
using System.Text.RegularExpressions;
using System.Threading;

namespace Shopfinity.Application.Features.Categories.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken ct = default);
    Task<CategoryDto>              GetCategoryByIdAsync(Guid id, CancellationToken ct = default);
    Task<CategoryDto>              CreateCategoryAsync(CreateCategoryDto dto, CancellationToken ct = default);
    Task<CategoryDto>              UpdateCategoryAsync(Guid id, CreateCategoryDto dto, CancellationToken ct = default);
    Task                           DeleteCategoryAsync(Guid id, CancellationToken ct = default);
}

public class CategoryService : ICategoryService
{
    private readonly IAppDbContext _context;
    private readonly IMapper       _mapper;

    public CategoryService(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper  = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken ct = default)
    {
        var cats = await _context.Categories.AsNoTracking().ToListAsync(ct);
        return _mapper.Map<IEnumerable<CategoryDto>>(cats);
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(Guid id, CancellationToken ct = default)
    {
        var cat = await _context.Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        return _mapper.Map<CategoryDto>(cat);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken ct = default)
    {
        var cat = _mapper.Map<Category>(dto);
        cat.Slug = await GenerateUniqueSlugAsync(dto.Name, null, ct);
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<CategoryDto>(cat);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(Guid id, CreateCategoryDto dto, CancellationToken ct = default)
    {
        var cat = await _context.Categories.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        cat.Name        = dto.Name;
        cat.Description = dto.Description;
        var baseForName = GenerateBaseSlug(dto.Name);
        if (string.IsNullOrEmpty(baseForName) || !cat.Slug.StartsWith(baseForName, StringComparison.Ordinal))
            cat.Slug = await GenerateUniqueSlugAsync(dto.Name, id, ct);

        await _context.SaveChangesAsync(ct);
        return _mapper.Map<CategoryDto>(cat);
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken ct = default)
    {
        var cat = await _context.Categories.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        cat.IsDeleted = true;
        await _context.SaveChangesAsync(ct);
    }

    private static string GenerateBaseSlug(string phrase)
    {
        var str = phrase.ToLowerInvariant();
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", " ").Trim();
        if (str.Length == 0) str = "category";
        str = str[..Math.Min(str.Length, 45)].Trim();
        str = Regex.Replace(str, @"\s", "-");
        return str;
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, Guid? excludeId, CancellationToken ct)
    {
        var baseSlug = GenerateBaseSlug(name);
        var slug     = baseSlug;
        var counter  = 1;
        while (await _context.Categories.AnyAsync(c =>
            c.Slug == slug && (excludeId == null || c.Id != excludeId), ct))
        {
            slug = $"{baseSlug}-{counter++}";
        }
        return slug;
    }
}
