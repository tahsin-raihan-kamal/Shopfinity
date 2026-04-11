import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { reviewService } from '@/services/reviewService'
import { CreateReviewDto } from '@/types'
import toast from 'react-hot-toast'

export function useProductReviews(productId: string) {
  return useQuery({
    queryKey: ['reviews', productId],
    queryFn: () => reviewService.getProductReviews(productId),
    enabled: !!productId,
    staleTime: 60 * 1000 // Cache for snappy re-renders
  })
}

export function useAddReview() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (dto: CreateReviewDto) => reviewService.add(dto),
    onSuccess: (_, variables) => {
      toast.success('Review submitted successfully')
      queryClient.invalidateQueries({ queryKey: ['reviews', variables.productId] })
    },
    onError: (err: any) => {
      // Typically the API Response contains failure details if multiple reviews etc.
      toast.error(err?.response?.data?.message || 'Failed to submit review')
    }
  })
}
