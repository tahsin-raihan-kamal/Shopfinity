'use client'

import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { categorySchema } from '@/lib/schemas'
import { useCategories, useCreateCategory, useUpdateCategory, useDeleteCategory } from '@/hooks/useCategories'
import { CategoryDto } from '@/types'
import { Input } from '@/components/ui/Input'
import { Button } from '@/components/ui/Button'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import toast from 'react-hot-toast'

type CategoryFormValues = z.infer<typeof categorySchema>

export default function AdminCategoriesPage() {
  const { data: categories = [], isLoading }   = useCategories()
  const createMutation = useCreateCategory()
  const updateMutation = useUpdateCategory()
  const deleteMutation = useDeleteCategory()

  const [editId, setEditId]         = useState<string | null>(null)
  const [showForm, setShowForm]     = useState(false)

  const { register, handleSubmit, reset, setValue, formState: { errors, isSubmitting } } =
    useForm<CategoryFormValues>({ resolver: zodResolver(categorySchema) })

  const openCreate = () => { reset(); setEditId(null); setShowForm(true) }
  const openEdit = (c: CategoryDto) => {
    setEditId(c.id); setValue('name', c.name); setValue('description', c.description); setShowForm(true)
  }

  const onSubmit = async (data: CategoryFormValues) => {
    try {
      const payload = { ...data, description: data.description ?? '' }
      if (editId) { await updateMutation.mutateAsync({ id: editId, dto: payload }); toast.success('Category updated!') }
      else        { await createMutation.mutateAsync(payload);                      toast.success('Category created!') }
      setShowForm(false); reset(); setEditId(null);
    } catch (e: unknown) { toast.error(e instanceof Error ? e.message : 'Failed') }
  }

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`Delete "${name}"?`)) return
    try { await deleteMutation.mutateAsync(id); toast.success('Deleted') }
    catch { toast.error('Delete failed') }
  }

  if (isLoading) return <div className="flex justify-center py-40"><LoadingSpinner size="lg" /></div>

  return (
    <div className="p-8">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-black text-gray-900 tracking-tight">Product Categories</h1>
          <p className="text-gray-500 font-medium mt-1">Organize your store layout and navigation structure.</p>
        </div>
        <Button onClick={openCreate} className="shadow-md">
           <span className="mr-2 text-lg">+</span> Add Category
        </Button>
      </div>

      {showForm && (
        <div className="mb-8 rounded-2xl border border-gray-200 bg-white p-8 shadow-sm max-w-2xl">
          <h2 className="text-xl font-bold text-gray-900 mb-6 border-b border-gray-100 pb-4">{editId ? 'Edit Category' : 'New Category Formation'}</h2>
          <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6">
            <Input id="catName" label="Category Name" {...register('name')} error={errors.name?.message} />
            <div>
              <label className="text-sm font-bold text-gray-700 block mb-2">Detailed Description</label>
              <textarea {...register('description')} rows={3} className="w-full rounded-xl bg-gray-50 border border-gray-200 px-4 py-3 text-gray-900 placeholder-gray-400 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 resize-none font-medium transition-all" placeholder="Optional description..."/>
            </div>
            <div className="flex justify-end gap-3 pt-4 border-t border-gray-100 mt-2">
              <Button variant="ghost" type="button" onClick={() => setShowForm(false)} className="text-gray-600 hover:text-gray-900 hover:bg-gray-100">Cancel Actions</Button>
              <Button type="submit" isLoading={isSubmitting} className="shadow-md">{editId ? 'Save Changes' : 'Initialize Category'}</Button>
            </div>
          </form>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {categories.map((c: CategoryDto) => (
          <div key={c.id} className="rounded-2xl border border-gray-200 bg-white p-6 flex flex-col gap-3 shadow-sm hover:shadow-md transition-shadow group">
            <div className="flex items-start justify-between">
              <h3 className="font-black text-gray-900 text-lg group-hover:text-indigo-600 transition-colors">{c.name}</h3>
              <div className="flex gap-3 flex-shrink-0 opacity-0 group-hover:opacity-100 transition-opacity">
                <button onClick={() => openEdit(c)} title="Edit" className="text-sm text-indigo-500 hover:text-indigo-800 transition-colors">✏️</button>
                <button onClick={() => handleDelete(c.id, c.name)} title="Delete" className="text-sm text-red-400 hover:text-red-700 transition-colors">🗑️</button>
              </div>
            </div>
            <p className="text-sm font-medium text-gray-500 line-clamp-3">{c.description || 'No description provided.'}</p>
          </div>
        ))}
        {categories.length === 0 && (
          <div className="col-span-full text-center py-16 flex flex-col items-center">
            <span className="text-4xl opacity-50 mb-4">🏷️</span>
            <p className="text-gray-900 font-bold">No categories exist.</p>
            <p className="text-gray-500 text-sm mt-1">Start by creating product sections.</p>
          </div>
        )}
      </div>
    </div>
  )
}
