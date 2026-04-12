import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { cartService } from '@/services/cartService'
import { AddToCartDto, CartItemDto } from '@/types'
import { useAuth } from '@/context/AuthContext'
import toast from 'react-hot-toast'

export function useCart() {
  const { user } = useAuth()
  const queryClient = useQueryClient()

  // Fetch Cart
  const { data: cart, isLoading, refetch } = useQuery({
    queryKey: ['cart'],
    queryFn: () => cartService.getCart(),
    enabled: !!user,
  })

  const items = cart?.items || []
  const totalPrice = cart?.totalPrice || 0
  const itemCount  = items.reduce((sum: number, i: CartItemDto) => sum + i.quantity, 0)

  // Mutations
  const addItemMutation = useMutation({
    mutationFn: (dto: AddToCartDto) => cartService.addItem(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] })
      toast.success('Added to cart!')
    },
    onError: () => toast.error('Failed to add item.')
  })

  const removeItemMutation = useMutation({
    mutationFn: (itemId: string) => cartService.removeItem(itemId),
    onMutate: async (itemId: string) => {
      // Optimistic update
      await queryClient.cancelQueries({ queryKey: ['cart'] })
      const previous = queryClient.getQueryData(['cart'])
      
      queryClient.setQueryData(['cart'], (old: any) => {
        if (old == null) return old
        return {
          ...old,
          items: (old?.items || []).filter((item: any) => item.id !== itemId),
        }
      })
      
      return { previous }
    },
    onError: (err: any, itemId: string, context: any) => {
      queryClient.setQueryData(['cart'], context?.previous)
      toast.error('Failed to remove item. Please try again.')
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] })
    }
  })

  return {
    items,
    totalPrice,
    itemCount,
    isLoading,
    addItem: (dto: AddToCartDto) => addItemMutation.mutateAsync(dto),
    removeItem: (itemId: string) => removeItemMutation.mutateAsync(itemId),
    refreshCart: () => refetch(),
    clearCart: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] })
    },
  }
}
