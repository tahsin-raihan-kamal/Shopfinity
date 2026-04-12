'use client'

import { useState } from 'react'
import { useDebounce } from '@/hooks/useDebounce'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { productSchema } from '@/lib/schemas'
import { useProducts, useCreateProduct, useUpdateProduct, useDeleteProduct } from '@/hooks/useProducts'
import { useCategories } from '@/hooks/useCategories'
import { ProductDto } from '@/types'
import { Input } from '@/components/ui/Input'
import { Button } from '@/components/ui/Button'
import { ImageUpload } from '@/components/ImageUpload'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import toast from 'react-hot-toast'
import { formatPrice } from '@/lib/formatters'
import Image from 'next/image'
import { getFullImageUrl } from '@/lib/utils'

export default function AdminProductsPage() {
  const [adminSearch, setAdminSearch] = useState('')
  const debouncedAdminSearch = useDebounce(adminSearch, 300)
  const { data: pData, isLoading: loadingP } = useProducts({
    pageSize: 100,
    pageNumber: 1,
    search: debouncedAdminSearch || undefined,
  })
  const { data: categories = [], isLoading: loadingC } = useCategories()
  const createMutation = useCreateProduct()
  const updateMutation = useUpdateProduct()
  const deleteMutation = useDeleteProduct()

  const products = pData?.items || []
  const isLoading = loadingP || loadingC

  const [editId, setEditId]       = useState<string | null>(null)
  const [showForm, setShowForm]   = useState(false)
  const [imageUrl, setImageUrl]   = useState('')

  const { register, handleSubmit, reset, setValue, formState: { errors, isSubmitting } } = useForm<z.infer<typeof productSchema>>({
    resolver: zodResolver(productSchema),
  })

  const openCreate = () => { reset(); setEditId(null); setImageUrl(''); setShowForm(true) }
  const openEdit = (p: ProductDto) => {
    setEditId(p.id)
    setImageUrl(p.imageUrl ?? '')
    setValue('name', p.name); setValue('slug', p.slug); setValue('description', p.description)
    setValue('price', p.price); setValue('stockQuantity', p.stockQuantity)
    setValue('categoryId', p.categoryId)
    setShowForm(true)
  }

  const onSubmit = async (data: z.infer<typeof productSchema>) => {
    try {
      const payload = { ...data, imageUrl, description: data.description ?? '' }
      if (editId) {
        await updateMutation.mutateAsync({ id: editId, dto: payload })
        toast.success('Product updated!')
      } else {
        await createMutation.mutateAsync(payload)
        toast.success('Product created!')
      }
      setShowForm(false); reset(); setEditId(null)
    } catch (e: unknown) {
      toast.error(e instanceof Error ? e.message : 'Failed')
    }
  }

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`Delete "${name}"?`)) return
    try {
      await deleteMutation.mutateAsync(id)
      toast.success('Deleted')
    } catch {
      toast.error('Delete failed')
    }
  }

  if (isLoading) return <div className="flex justify-center py-40"><LoadingSpinner size="lg" /></div>

  return (
    <div className="p-8">
      <div className="flex flex-col gap-6 sm:flex-row sm:items-center sm:justify-between mb-8">
        <div>
          <h1 className="text-3xl font-black text-gray-900 tracking-tight">Products</h1>
          <p className="text-gray-500 font-medium mt-1">Manage your storefront inventory.</p>
        </div>
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:gap-4 w-full sm:w-auto">
          <input
            type="search"
            value={adminSearch}
            onChange={e => setAdminSearch(e.target.value)}
            placeholder="Search by name…"
            className="w-full sm:w-64 rounded-xl border border-gray-200 bg-white px-4 py-2.5 text-sm font-medium text-gray-900 placeholder:text-gray-400 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/30"
            aria-label="Filter products by name"
          />
          <Button onClick={openCreate} className="shadow-md shrink-0">
            <span className="mr-2 text-lg">+</span> Add Product
          </Button>
        </div>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit(onSubmit)} className="bg-white p-8 rounded-2xl border border-gray-200 shadow-sm mb-12">
          <h2 className="text-2xl font-black text-gray-900 mb-6 pb-4 border-b border-gray-100">{editId ? 'Edit Product' : 'Create New Product'}</h2>
          
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            <div className="lg:col-span-2 space-y-6">
              <Input label="Product Name" {...register('name')} error={errors.name?.message} />
              <Input label="Slug (URL)" {...register('slug')} error={errors.slug?.message} />
              
              <div className="flex flex-col gap-1.5">
                <label className="text-sm font-bold text-gray-700">Description</label>
                <textarea 
                  {...register('description')} 
                  rows={4} 
                  className="rounded-xl border border-gray-300 px-4 py-3 bg-white text-gray-900 focus:outline-none focus:ring-2 focus:ring-indigo-600 focus:border-indigo-600 transition-all text-sm"
                />
              </div>

              <div className="grid grid-cols-2 gap-6">
                <Input label="Price (BDT)" type="number" step="0.01" {...register('price', { valueAsNumber: true })} error={errors.price?.message} />
                <Input label="Stock Quantity" type="number" {...register('stockQuantity', { valueAsNumber: true })} error={errors.stockQuantity?.message} />
              </div>
            </div>

            <div className="space-y-6">
              <div className="flex flex-col gap-1.5">
                <label className="text-sm font-bold text-gray-700">Category</label>
                <select 
                  {...register('categoryId')} 
                  className="rounded-xl border border-gray-300 px-4 py-3 bg-white text-gray-900 focus:outline-none focus:ring-2 focus:ring-indigo-600 transition-all text-sm font-medium"
                >
                  <option value="">Select Category</option>
                  {categories.map(c => (
                    <option key={c.id} value={c.id}>{c.name}</option>
                  ))}
                </select>
                {errors.categoryId && <p className="text-red-500 text-xs font-semibold mt-1">{errors.categoryId.message}</p>}
              </div>

              <div className="bg-gray-50 border border-gray-200 rounded-xl p-4">
                <label className="text-sm font-bold text-gray-700 block mb-3">Product Image</label>
                <ImageUpload value={imageUrl} onChange={(url) => setImageUrl(url)} />
              </div>
            </div>
          </div>

          <div className="mt-8 flex gap-4 pt-6 border-t border-gray-100 justify-end">
            <Button type="button" variant="ghost" onClick={() => setShowForm(false)}>Cancel</Button>
            <Button type="submit" disabled={isSubmitting} className="min-w-[120px]">
              {isSubmitting ? <LoadingSpinner size="sm" /> : 'Save Product'}
            </Button>
          </div>
        </form>
      )}

      {/* Modern Data Table */}
      <div className="bg-white rounded-2xl border border-gray-200 shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm text-gray-600">
            <thead className="bg-gray-50 text-gray-500 uppercase font-black text-xs tracking-wider border-b border-gray-200">
              <tr>
                <th className="px-6 py-4">Product</th>
                <th className="px-6 py-4 hidden sm:table-cell">Category</th>
                <th className="px-6 py-4 hidden md:table-cell">Stock</th>
                <th className="px-6 py-4">Price</th>
                <th className="px-6 py-4 text-center">Status</th>
                <th className="px-6 py-4 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {products.map(p => {
                const thumb = p.imageUrl ? getFullImageUrl(p.imageUrl) : ''
                return (
                <tr key={p.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-6 py-4 min-w-[250px]">
                    <div className="flex items-center gap-4">
                       <div className="w-12 h-12 bg-white rounded-lg border border-gray-200 p-1 flex-shrink-0 relative overflow-hidden">
                          {thumb ? (
                            <Image
                              src={thumb}
                              fill
                              className="object-contain mix-blend-multiply"
                              alt=""
                              unoptimized={thumb.startsWith('http://localhost') || thumb.startsWith('http://127.0.0.1')}
                            />
                          ) : (
                            <span className="text-xl">📦</span>
                          )}
                       </div>
                       <div>
                         <p className="font-bold text-gray-900 text-base">{p.name}</p>
                         <p className="text-xs text-gray-400 font-medium">/{p.slug}</p>
                       </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 hidden sm:table-cell font-medium">
                    {categories.find(c => c.id === p.categoryId)?.name ?? 'N/A'}
                  </td>
                  <td className="px-6 py-4 hidden md:table-cell">
                    <span className={`px-3 py-1 rounded-full text-xs font-black ${p.stockQuantity > 0 ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                      {p.stockQuantity}
                    </span>
                  </td>
                  <td className="px-6 py-4 font-black text-gray-900 text-base">
                    {formatPrice(p.price)}
                  </td>
                  <td className="px-6 py-4 text-center">
                    {p.stockQuantity > 0 
                      ? <span className="text-green-500 font-black">●</span> 
                      : <span className="text-gray-300 font-black">●</span>
                    }
                  </td>
                  <td className="px-6 py-4 text-right">
                    <div className="flex items-center justify-end gap-3 opacity-0 group-hover:opacity-100 md:opacity-100 transition-opacity">
                      <button onClick={() => openEdit(p)} className="text-indigo-600 hover:text-indigo-800 font-bold transition-colors">Edit</button>
                      <button onClick={() => handleDelete(p.id, p.name)} className="text-red-500 hover:text-red-700 font-bold transition-colors">Delete</button>
                    </div>
                  </td>
                </tr>
                )
              })}
              {products.length === 0 && (
                <tr>
                  <td colSpan={6} className="px-6 py-12 text-center text-gray-500 font-medium">
                    No products found. Start by creating one above.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}
