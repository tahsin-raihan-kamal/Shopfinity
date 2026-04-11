'use client'

import { useWishlist, useRemoveWishlistItem } from '@/hooks/useWishlist'
import { useCart } from '@/hooks/useCart'
import { EmptyState } from '@/components/EmptyState'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { formatPrice } from '@/lib/formatters'
import { getFullImageUrl } from '@/lib/utils'
import Link from 'next/link'
import Image from 'next/image'
import toast from 'react-hot-toast'
import { useAuth } from '@/context/AuthContext'
import { useEffect } from 'react'
import { useRouter } from 'next/navigation'

export default function WishlistPage() {
  const { user } = useAuth()
  const router = useRouter()
  
  // Protect route
  useEffect(() => {
    if (user === null) {
      router.push('/login?redirect=/wishlist')
    }
  }, [user, router])

  const { data: wishlistItems, isLoading } = useWishlist()
  const removeMutation = useRemoveWishlistItem()
  const { addItem } = useCart()

  const handleRemove = async (id: string, name: string) => {
    await removeMutation.mutateAsync(id)
    toast.success(`Removed ${name} from Wishlist`)
  }

  const handleMoveToCart = async (productId: string, productName: string, wishlistItemId: string) => {
    await addItem({ productId, quantity: 1 })
    toast.success(`Added ${productName} to Cart`)
    await removeMutation.mutateAsync(wishlistItemId)
  }

  if (user === null) return null // handle redirect visual flash
  
  if (isLoading) {
    return <div className="flex justify-center items-center py-40"><LoadingSpinner size="lg" /></div>
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 py-12">
      <h1 className="text-3xl font-black text-gray-900 tracking-tight mb-8">My Wishlist</h1>

      {(!wishlistItems || wishlistItems.length === 0) ? (
        <div className="bg-gray-50 rounded-3xl p-12 text-center border border-gray-200 shadow-sm mt-8">
          <span className="text-7xl block mb-6 drop-shadow-sm">❤️</span>
          <h2 className="text-2xl font-black text-gray-900 mb-3 tracking-tight">Your Wishlist is Empty</h2>
          <p className="text-gray-500 mb-8 max-w-md mx-auto">Found something you like but not ready to buy? Tap the heart icon on any gear and it will show up here.</p>
          <Link href="/" className="inline-block px-8 py-4 bg-indigo-600 hover:bg-indigo-700 text-white font-bold rounded-xl shadow-lg transition-transform hover:-translate-y-0.5">
            Start Exploring
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
          {wishlistItems.map((item) => (
            <div key={item.id} className="group flex flex-col rounded-2xl bg-white border border-gray-200 overflow-hidden shadow-sm hover:shadow-xl transition-all duration-300">
              <Link href={item.productSlug ? `/products/${item.productSlug}` : '/'} className="relative h-64 bg-gray-50 p-6 flex justify-center items-center overflow-hidden">
                {item.productImageUrl ? (
                   <Image
                     src={getFullImageUrl(item.productImageUrl)}
                     alt={item.productName}
                     fill
                     sizes="(max-width: 768px) 100vw, 25vw"
                     className="object-contain p-6 mix-blend-multiply group-hover:scale-105 transition-transform duration-500"
                     unoptimized={getFullImageUrl(item.productImageUrl).startsWith('http://localhost') || getFullImageUrl(item.productImageUrl).startsWith('http://127.0.0.1')}
                   />
                ) : (
                   <span className="text-5xl opacity-20">📦</span>
                )}
                {/* Remove button absolute */}
                <button 
                  onClick={(e) => { e.preventDefault(); handleRemove(item.id, item.productName) }}
                  className="absolute top-4 right-4 h-10 w-10 flex items-center justify-center bg-white rounded-full shadow-md text-gray-400 hover:text-red-500 transition-colors z-10"
                  aria-label="Remove from Wishlist"
                  title="Remove from Wishlist"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" /></svg>
                </button>
              </Link>
              
              <div className="p-5 flex flex-col flex-1 border-t border-gray-100 bg-white">
                <Link href={item.productSlug ? `/products/${item.productSlug}` : '/'}>
                  <h3 className="font-bold text-gray-900 text-sm line-clamp-2 hover:text-indigo-600 transition-colors mb-2 leading-snug">{item.productName}</h3>
                </Link>
                <div className="font-black text-gray-900 text-lg mb-4 mt-auto">{formatPrice(item.price)}</div>
                
                <button 
                  onClick={() => handleMoveToCart(item.productId, item.productName, item.id)}
                  className="w-full py-3 bg-gray-900 text-white font-bold rounded-xl text-sm hover:bg-black transition-colors"
                >
                  Move to Cart
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
