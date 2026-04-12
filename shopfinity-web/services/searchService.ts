import api from '@/lib/axios'
import { ProductSearchSuggestionDto } from '@/types'

export async function fetchSearchSuggestions(q: string): Promise<ProductSearchSuggestionDto[]> {
  const term = q.trim()
  if (!term) return []
  const res = await api.get<ProductSearchSuggestionDto[]>('/api/v1/Products/search', {
    params: { q: term },
  })
  return res.data ?? []
}
