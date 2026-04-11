import api from '@/lib/axios'
import { CategoryDto, CreateCategoryDto } from '@/types'

export const categoryService = {
  async getAll(): Promise<CategoryDto[]> {
    const res = await api.get<CategoryDto[]>('/api/v1/Categories')
    return res.data
  },
  async create(dto: CreateCategoryDto): Promise<CategoryDto> {
    const res = await api.post<CategoryDto>('/api/v1/Categories', dto)
    return res.data
  },
  async update(id: string, dto: CreateCategoryDto): Promise<CategoryDto> {
    const res = await api.put<CategoryDto>(`/api/v1/Categories/${id}`, dto)
    return res.data
  },
  async delete(id: string): Promise<void> {
    await api.delete(`/api/v1/Categories/${id}`)
  },
}
