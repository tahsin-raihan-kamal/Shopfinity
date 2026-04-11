import api from '@/lib/axios'
import { ProductDto, CreateProductDto, PaginatedResult, ProductSearchDto } from '@/types'

export const productService = {
  async search(params: ProductSearchDto): Promise<PaginatedResult<ProductDto>> {
    const query = new URLSearchParams()
    if (params.search?.trim()) query.set('search', params.search.trim())
    if (params.searchTerm)  query.set('searchTerm',  params.searchTerm)
    if (params.categoryId)  query.set('categoryId',  params.categoryId)
    if (params.categorySlug) query.set('categorySlug', params.categorySlug)
    if (params.minPrice != null) query.set('minPrice', String(params.minPrice))
    if (params.maxPrice != null) query.set('maxPrice', String(params.maxPrice))
    query.set('pageNumber', String(params.pageNumber ?? 1))
    query.set('pageSize',   String(params.pageSize   ?? 12))

    const res = await api.get<PaginatedResult<ProductDto>>(`/api/v1/Products?${query}`)
    return res.data
  },

  async getBySlug(slug: string): Promise<ProductDto> {
    const res = await api.get<ProductDto>(`/api/v1/Products/${slug}`)
    return res.data
  },

  async create(dto: CreateProductDto): Promise<ProductDto> {
    const res = await api.post<ProductDto>('/api/v1/Products', dto)
    return res.data
  },

  async update(id: string, dto: CreateProductDto): Promise<ProductDto> {
    const res = await api.put<ProductDto>(`/api/v1/Products/${id}`, dto)
    return res.data
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/api/v1/Products/${id}`)
  },
}
