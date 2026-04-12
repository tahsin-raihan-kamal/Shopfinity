import { useQuery } from '@tanstack/react-query'
import { fetchSearchSuggestions } from '@/services/searchService'

/**
 * Debounced query should be passed from the caller (e.g. useDebounce(term, 300)).
 */
export function useSearchSuggestions(debouncedQuery: string) {
  const q = debouncedQuery.trim()
  return useQuery({
    queryKey: ['product-search-suggestions', q],
    queryFn: () => fetchSearchSuggestions(q),
    enabled: q.length >= 1,
    staleTime: 20_000,
  })
}
