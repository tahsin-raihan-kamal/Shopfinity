import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { categoryService } from '@/services/categoryService'
import { CategoryDto } from '@/types'

export function useCategories() {
  return useQuery({
    queryKey: ['categories'],
    queryFn: () => categoryService.getAll(),
    staleTime: 5 * 60 * 1000 // Cache for 5 mins as it rarely changes
  })
}

export function useCreateCategory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (dto: any) => categoryService.create(dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['categories'] })
  })
}

export function useUpdateCategory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, dto }: { id: string, dto: any }) => categoryService.update(id, dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['categories'] })
  })
}

export function useDeleteCategory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => categoryService.delete(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['categories'] })
  })
}
