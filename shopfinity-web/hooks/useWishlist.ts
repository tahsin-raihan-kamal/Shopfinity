import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { wishlistService } from '@/services/wishlistService'
import { AddWishlistItemDto, WishlistItemDto } from '@/types'

export function useWishlist() {
  return useQuery({
    queryKey: ['wishlist'],
    queryFn: () => wishlistService.getMyWishlist(),
    staleTime: 60 * 1000 // 1 minute stale time for stale-while-revalidate UX
  })
}

export function useAddWishlistItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (dto: AddWishlistItemDto) => wishlistService.add(dto),
    onMutate: async (newItem) => {
      await queryClient.cancelQueries({ queryKey: ['wishlist'] })
      const previousWishlist = queryClient.getQueryData<WishlistItemDto[]>(['wishlist'])
      
      // Optimistic update
      if (previousWishlist) {
        queryClient.setQueryData<WishlistItemDto[]>(['wishlist'], [
          {
            id: 'temp-' + Date.now(),
            productId: newItem.productId,
            productSlug: '',
            productName: '',
            productImageUrl: '',
            price: 0,
            createdAt: new Date().toISOString()
          },
          ...previousWishlist
        ])
      }
      return { previousWishlist }
    },
    onError: (_err, _newItem, context) => {
      if (context?.previousWishlist) {
        queryClient.setQueryData(['wishlist'], context.previousWishlist)
      }
      // HTTP errors: message shown by axios interceptor (409 / 404 / validation)
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['wishlist'] })
    }
  })
}

export function useRemoveWishlistItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => wishlistService.remove(id),
    onMutate: async (id) => {
      await queryClient.cancelQueries({ queryKey: ['wishlist'] })
      const previousWishlist = queryClient.getQueryData<WishlistItemDto[]>(['wishlist'])
      
      // Optimistic update
      if (previousWishlist) {
        queryClient.setQueryData<WishlistItemDto[]>(
          ['wishlist'], 
          previousWishlist.filter(item => item.id !== id)
        )
      }
      return { previousWishlist }
    },
    onError: (_err, _id, context) => {
      if (context?.previousWishlist) {
        queryClient.setQueryData(['wishlist'], context.previousWishlist)
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['wishlist'] })
    }
  })
}
