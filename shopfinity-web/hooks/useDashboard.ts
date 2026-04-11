import { useQuery } from '@tanstack/react-query'
import api from '@/lib/axios'

interface DashboardMetrics {
  totalUsers: number
  totalOrders: number
  totalRevenue: number
  dailySales: number
}

export function useDashboardMetrics() {
  return useQuery({
    queryKey: ['admin-dashboard-metrics'],
    queryFn: async () => {
      const res = await api.get<DashboardMetrics>('/api/v1/Dashboard')
      return res.data
    }
  })
}
