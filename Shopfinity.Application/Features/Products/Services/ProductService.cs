using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Shopfinity.Application.Common;
using Shopfinity.Application.Common.Interfaces;
using Shopfinity.Application.Features.Products.DTOs;
using Shopfinity.Domain.Entities;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using Npgsql;

namespace Shopfinity.Application.Features.Products.Services;

public interface IProductService
{
    Task<PaginatedResult<ProductDto>> SearchProductsAsync(ProductSearchDto searchDto, CancellationToken ct = default);
    Task<IReadOnlyList<ProductSearchSuggestionDto>> SearchSuggestionsAsync(string? query, CancellationToken ct = default);
    Task<ProductDto>                  GetProductBySlugAsync(string slug, CancellationToken ct = default);
    Task<ProductDto>                  CreateProductAsync(CreateProductDto dto, CancellationToken ct = default);
    Task<ProductDto>                  UpdateProductAsync(Guid id, CreateProductDto dto, CancellationToken ct = default);
    Task                              DeleteProductAsync(Guid id, CancellationToken ct = default);
}

public class ProductService : IProductService
{
    private readonly IAppDbContext  _context;
    private readonly IMapper        _mapper;
    private readonly IMemoryCache   _cache;

    public ProductService(IAppDbContext context, IMapper mapper, IMemoryCache cache)
    {
        _context = context;
        _mapper  = mapper;
        _cache   = cache;
    }

    private bool UsePostgresFullTextSearch =>
        _context is DbContext db &&
        db.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";

    // ── Search / List ─────────────────────────────────────────────────────────
    public async Task<PaginatedResult<ProductDto>> SearchProductsAsync(ProductSearchDto searchDto, CancellationToken ct = default)
    {
        var query = _context.Products.AsNoTracking().AsQueryable();

        var rawSearch = searchDto.SearchTerm ?? searchDto.Search;
        if (!string.IsNullOrWhiteSpace(rawSearch))
        {
            var term = rawSearch.Trim();
            if (UsePostgresFullTextSearch)
            {
                query = query.Where(p =>
                    p.SearchVector != null &&
                    p.SearchVector.Matches(EF.Functions.PlainToTsQuery("english", term)));
            }
            else
            {
                var t = term.ToLowerInvariant();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(t) ||
                    p.Description.ToLower().Contains(t));
            }
        }

        if (searchDto.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == searchDto.CategoryId.Value);
        else if (!string.IsNullOrWhiteSpace(searchDto.CategorySlug))
        {
            var slug = searchDto.CategorySlug.Trim().ToLowerInvariant();
            query = query.Where(p => _context.Categories.Any(c => c.Id == p.CategoryId && c.Slug.ToLower() == slug));
        }

        if (searchDto.MinPrice.HasValue)
            query = query.Where(p => p.Price >= searchDto.MinPrice.Value);

        if (searchDto.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= searchDto.MaxPrice.Value);

        var totalCount = await query.CountAsync(ct);

        // Optional Sort By Relevance vs Standard sorts
        if (!string.IsNullOrWhiteSpace(rawSearch))
        {
            var term = rawSearch.Trim();
            query = searchDto.SortBy?.ToLower() switch
            {
                "priceasc" => query.OrderBy(p => p.Price),
                "pricedesc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => UsePostgresFullTextSearch
                    ? query.OrderByDescending(p => p.SearchVector!.Rank(EF.Functions.PlainToTsQuery("english", term)))
                    : query.OrderBy(p => p.Name)
            };
        }
        else
        {
            query = searchDto.SortBy?.ToLower() switch
            {
                "priceasc" => query.OrderBy(p => p.Price),
                "pricedesc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Name)
            };
        }

        var products = await query
            .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToListAsync(ct);

        return new PaginatedResult<ProductDto>
        {
            Items      = _mapper.Map<IEnumerable<ProductDto>>(products),
            TotalCount = totalCount,
            PageNumber = searchDto.PageNumber,
            PageSize   = searchDto.PageSize
        };
    }

    public async Task<IReadOnlyList<ProductSearchSuggestionDto>> SearchSuggestionsAsync(string? query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<ProductSearchSuggestionDto>();
        var term = query.Trim();
        if (term.Length > 200) term = term[..200];

        return UsePostgresFullTextSearch
            ? await SearchSuggestionsPostgresAsync(term, ct)
            : await SearchSuggestionsFallbackAsync(term, ct);
    }

    private async Task<IReadOnlyList<ProductSearchSuggestionDto>> SearchSuggestionsPostgresAsync(string term, CancellationToken ct)
    {
        var db   = (DbContext)_context;
        var conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
SELECT p."Id", p."Name", p."Price", p."ImageUrl", p."Slug"
FROM "Products" p
WHERE NOT p."IsDeleted"
  AND (
    lower(p."Name") LIKE '%' || lower(@p) || '%'
    OR (char_length(@p::text) >= 3 AND similarity(p."Name", @p::text) > 0.12)
  )
ORDER BY
  CASE WHEN lower(p."Name") = lower(@p) THEN 1
       WHEN lower(p."Name") LIKE lower(@p) || '%' THEN 2
       WHEN lower(p."Name") LIKE '%' || lower(@p) || '%' THEN 3
       ELSE 4 END,
  similarity(p."Name", @p::text) DESC NULLS LAST,
  p."Name"
LIMIT 10
""";
        cmd.Parameters.Add(new NpgsqlParameter("p", term));

        var list = new List<ProductSearchSuggestionDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(new ProductSearchSuggestionDto
            {
                Id       = reader.GetGuid(0),
                Name     = reader.GetString(1),
                Price    = reader.GetDecimal(2),
                ImageUrl = reader.IsDBNull(3) ? null : reader.GetString(3),
                Slug     = reader.GetString(4)
            });
        }

        return list;
    }

    private async Task<IReadOnlyList<ProductSearchSuggestionDto>> SearchSuggestionsFallbackAsync(string term, CancellationToken ct)
    {
        var t = term.ToLowerInvariant();
        var candidates = await _context.Products.AsNoTracking()
            .Where(p => p.Name.ToLower().Contains(t) || p.Description.ToLower().Contains(t))
            .Take(100)
            .ToListAsync(ct);

        static int RankName(string name, string needle)
        {
            if (string.Equals(name, needle, StringComparison.OrdinalIgnoreCase)) return 1;
            if (name.StartsWith(needle, StringComparison.OrdinalIgnoreCase)) return 2;
            if (name.Contains(needle, StringComparison.OrdinalIgnoreCase)) return 3;
            return 10;
        }

        return candidates
            .Select(p => (p, r: RankName(p.Name, term)))
            .OrderBy(x => x.r)
            .ThenBy(x => x.p.Name)
            .Take(10)
            .Select(x => new ProductSearchSuggestionDto
            {
                Id       = x.p.Id,
                Name     = x.p.Name,
                Price    = x.p.Price,
                ImageUrl = x.p.ImageUrl,
                Slug     = x.p.Slug
            })
            .ToList();
    }

    // ── Get by Slug ───────────────────────────────────────────────────────────
    public async Task<ProductDto> GetProductBySlugAsync(string slug, CancellationToken ct = default)
    {
        var cacheKey = $"Product_{slug}";

        if (!_cache.TryGetValue(cacheKey, out ProductDto? cached))
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Slug == slug, ct);

            if (product == null)
                throw new KeyNotFoundException($"Product with slug '{slug}' not found.");

            cached = _mapper.Map<ProductDto>(product);

            _cache.Set(cacheKey, cached, new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10)));
        }

        return cached!;
    }

    // ── Create ────────────────────────────────────────────────────────────────
    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        var product  = _mapper.Map<Product>(dto);
        product.Slug = await GenerateUniqueSlugAsync(product.Name, null, ct);

        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<ProductDto>(product);
    }

    // ── Update ────────────────────────────────────────────────────────────────
    public async Task<ProductDto> UpdateProductAsync(Guid id, CreateProductDto dto, CancellationToken ct = default)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        product.Name          = dto.Name;
        product.Description   = dto.Description;
        product.Price         = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.CategoryId    = dto.CategoryId;
        product.ImageUrl      = dto.ImageUrl;
        product.Tags          = dto.Tags ?? new List<string>();

        // Regenerate slug if name changed
        if (!product.Slug.StartsWith(GenerateBaseSlug(dto.Name)))
            product.Slug = await GenerateUniqueSlugAsync(dto.Name, id, ct);

        await _context.SaveChangesAsync(ct);

        // Invalidate cache
        _cache.Remove($"Product_{product.Slug}");

        return _mapper.Map<ProductDto>(product);
    }

    // ── Delete (soft) ─────────────────────────────────────────────────────────
    public async Task DeleteProductAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        product.IsDeleted = true;
        _cache.Remove($"Product_{product.Slug}");
        await _context.SaveChangesAsync(ct);
    }

    // ── Slug helpers ─────────────────────────────────────────────────────────
    private static string GenerateBaseSlug(string phrase)
    {
        var str = phrase.ToLowerInvariant();
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", " ").Trim();
        str = str[..Math.Min(str.Length, 45)].Trim();
        str = Regex.Replace(str, @"\s", "-");
        return str;
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        var baseSlug = GenerateBaseSlug(name);
        var slug     = baseSlug;
        var counter  = 1;

        while (await _context.Products.AnyAsync(p =>
            p.Slug == slug && (excludeId == null || p.Id != excludeId), ct))
        {
            slug = $"{baseSlug}-{counter++}";
        }

        return slug;
    }
}
