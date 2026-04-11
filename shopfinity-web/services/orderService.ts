import api from '@/lib/axios'
import { OrderResponseDto, UpdateOrderStatusDto } from '@/types'

export const orderService = {
  async getMyOrders(): Promise<OrderResponseDto[]> {
    const res = await api.get<OrderResponseDto[]>('/api/v1/Orders')
    return res.data
  },
  async checkout(): Promise<OrderResponseDto> {
    const res = await api.post<OrderResponseDto>('/api/v1/Orders/checkout', {})
    return res.data
  },
  async getAllOrders(): Promise<OrderResponseDto[]> {
    const res = await api.get<OrderResponseDto[]>('/api/v1/Orders/admin/all')
    return res.data
  },
  async updateStatus(id: string, dto: UpdateOrderStatusDto): Promise<OrderResponseDto> {
    const res = await api.put<OrderResponseDto>(`/api/v1/Orders/${id}/status`, dto)
    return res.data
  },
}
