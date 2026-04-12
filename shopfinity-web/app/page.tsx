'use client'

import { useState, useEffect, useRef } from 'react'
import { useSearchParams, useRouter } from 'next/navigation'
import { ProductDto, CategoryDto } from '@/types'
import { ProductCard } from '@/components/ProductCard'
import { Pagination } from '@/components/Pagination'
import { EmptyState } from '@/components/EmptyState'
import { useDebounce } from '@/hooks/useDebounce'
import { useCart } from '@/hooks/useCart'
import { useAuth } from '@/context/AuthContext'
import { useProducts } from '@/hooks/useProducts'
import { useCategories } from '@/hooks/useCategories'
import toast from 'react-hot-toast'
import Link from 'next/link'
import Image from 'next/image'
import { ProductCardSkeleton } from '@/components/ui/Skeleton'

export default function HomePage() {
  const { user } = useAuth()
  const { addItem } = useCart()
  const searchParams = useSearchParams()
  const router = useRouter()

  const [page, setPage] = useState(1)

  const categorySlug = searchParams.get('categorySlug')?.trim() ?? ''
  const categoryIdParam = searchParams.get('categoryId')?.trim() ?? ''
  const search = searchParams.get('search')?.trim() ?? ''
  const filterKeyRef = useRef(`${categorySlug}|${categoryIdParam}|${search}`)

  const debouncedSearch = useDebounce(search, 400)

  useEffect(() => {
    const key = `${categorySlug}|${categoryIdParam}|${search}`
    if (key !== filterKeyRef.current) {
      filterKeyRef.current = key
      setPage(1)
    }
  }, [categorySlug, categoryIdParam, search])

  // React Query Hooks
  const { data: categories = [] } = useCategories()
  const { data: result, isLoading } = useProducts({
    searchTerm: debouncedSearch,
    categorySlug: categorySlug || undefined,
    categoryId: categorySlug ? undefined : (categoryIdParam || undefined),
    pageNumber: page,
    pageSize: 8
  })

  const setCategoryFilter = (slug: string, id: string) => {
    const next = new URLSearchParams(searchParams.toString())
    next.delete('categorySlug')
    next.delete('categoryId')
    if (slug) next.set('categorySlug', slug)
    else if (id) next.set('categoryId', id)
    const q = next.toString()
    router.push(q ? `/?${q}` : '/')
  }

  const handleAddToCart = async (product: ProductDto) => {
    if (!user) { toast.error('Please sign in to add items'); return }
    await addItem({ productId: product.id, quantity: 1 })
    toast.success('Added to Cart')
  }

  return (
    <div className="flex flex-col w-full">
      {/* Hero Banner (Only show if not searching/filtering heavily) */}
      {!search && !categorySlug && !categoryIdParam && page === 1 && (
        <div className="relative w-full bg-gray-900 border-b border-gray-800">
          <div className="absolute inset-0 overflow-hidden">
            <Image 
              src="https://img.freepik.com/premium-photo/futuristic-gaming-room-setup-with-neon-lights_220556-8200.jpg" 
              alt="Hero" 
              fill 
              priority
              className="object-cover opacity-40 mix-blend-overlay" 
            />
          </div>
          <div className="relative max-w-7xl mx-auto px-6 py-24 sm:py-32 flex flex-col items-start justify-center">
            <span className="text-indigo-400 font-bold tracking-widest uppercase mb-4 text-sm">New Arrivals</span>
            <h1 className="text-4xl sm:text-6xl font-black text-white leading-tight mb-6 max-w-3xl">
              Elevate Your Setup with Next-Gen Tech
            </h1>
            <p className="text-lg text-gray-300 max-w-xl mb-10">
              Discover premium laptops, mechanical keyboards, audiophile gear, and accessories designed for peak performance.
            </p>
            <div className="flex gap-4">
              <button onClick={() => window.scrollTo({top: 800, behavior: 'smooth'})} className="px-8 py-4 bg-white text-gray-900 font-bold rounded-lg hover:bg-gray-100 transition-colors shadow-lg">
                Shop Now
              </button>
              <Link href="/login" className="px-8 py-4 bg-transparent border-2 border-white text-white font-bold rounded-lg hover:bg-white/10 transition-colors">
                Join Shopfinity
              </Link>
            </div>
          </div>
        </div>
      )}

      {/* Categories Bar */}
      <div className="bg-white border-b border-gray-200 sticky top-16 z-30 shadow-sm">
        <div className="max-w-7xl mx-auto px-6 py-4 flex gap-4 overflow-x-auto no-scrollbar items-center">
          <button 
            onClick={() => setCategoryFilter('', '')}
            className={`whitespace-nowrap px-4 py-2 rounded-full text-sm font-bold transition-colors ${!categorySlug && !categoryIdParam ? 'bg-gray-900 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}
          >
            All Products
          </button>
          {categories.map((c: CategoryDto) => (
            <button 
              key={c.id}
              onClick={() => setCategoryFilter(c.slug || '', c.id)}
              className={`whitespace-nowrap px-4 py-2 rounded-full text-sm font-bold transition-colors ${(categorySlug && c.slug === categorySlug) || (!categorySlug && categoryIdParam === c.id) ? 'bg-gray-900 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}
            >
              {c.name}
            </button>
          ))}
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-6 py-12 w-full">
        {/* Search Bar */}
        <div className="mb-8">
          <h2 className="text-3xl font-black text-gray-900">
            {search ? `Searching for "${search}"` : (categorySlug || categoryIdParam) ? 'Category Results' : 'Trending Gear'}
          </h2>
        </div>

        {/* Results */}
        {isLoading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
            {[1,2,3,4,5,6,7,8].map(i => (
              <ProductCardSkeleton key={i} />
            ))}
          </div>
        ) : result?.items.length === 0 ? (
          <EmptyState icon="😕" title="No gear found" message="We couldn't find anything matching your search. Try adjusting the filters." />
        ) : (
          <>
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
              {result?.items.map((p: ProductDto) => (
                <ProductCard key={p.id} product={p} onAddToCart={() => handleAddToCart(p)} />
              ))}
            </div>
            
            <div className="mt-12 flex justify-center border-t border-gray-200 pt-8">
               <Pagination currentPage={page} totalPages={result?.totalPages ?? 1} onPageChange={setPage} />
            </div>
          </>
        )}
      </div>
    </div>
  )
}
