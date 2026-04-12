import api from '@/lib/axios'
import { ProductReviewDto, CreateReviewDto } from '@/types'

export const reviewService = {
  async getProductReviews(productId: string): Promise<ProductReviewDto[]> {
    const res = await api.get<ProductReviewDto[]>(`/api/v1/Reviews/product/${productId}`)
    return res.data
  },

  async add(dto: CreateReviewDto): Promise<ProductReviewDto> {
    const res = await api.post<ProductReviewDto>('/api/v1/Reviews', dto)
    return res.data
  }
}
