import api from '@/lib/axios'
import { CartDto, AddToCartDto } from '@/types'

export const cartService = {
  async getCart(): Promise<CartDto> {
    const res = await api.get<CartDto>('/api/v1/Carts')
    return res.data
  },
  async addItem(dto: AddToCartDto): Promise<void> {
    try {
      await api.post('/api/v1/Carts/items', dto)
    } catch (e: unknown) {
      const status = (e as { response?: { status?: number } })?.response?.status
      if (status === 409) {
        await api.get('/api/v1/Carts')
        return
      }
      throw e
    }
  },
  async removeItem(itemId: string): Promise<void> {
    await api.delete(`/api/v1/Carts/items/${itemId}`)
  },
}
