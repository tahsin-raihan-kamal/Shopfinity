import api from '@/lib/axios'
import { WishlistItemDto, AddWishlistItemDto } from '@/types'

export const wishlistService = {
  async getMyWishlist(): Promise<WishlistItemDto[]> {
    const res = await api.get<WishlistItemDto[]>('/api/v1/Wishlists')
    return res.data
  },

  async add(dto: AddWishlistItemDto): Promise<WishlistItemDto> {
    const res = await api.post<WishlistItemDto>('/api/v1/Wishlists', {
      productId: dto.productId,
    })
    return res.data
  },

  async remove(id: string): Promise<void> {
    await api.delete(`/api/v1/Wishlists/${id}`)
  }
}
