'use client'

import { useCart } from '@/hooks/useCart'
import { useAuth } from '@/context/AuthContext'
import { EmptyState } from '@/components/EmptyState'
import { useRouter } from 'next/navigation'
import { useEffect } from 'react'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import Link from 'next/link'
import Image from 'next/image'
import { formatPrice } from '@/lib/formatters'
import { getFullImageUrl } from '@/lib/utils'

export default function CartPage() {
  const { user, isLoading: authLoading } = useAuth()
  const { items, totalPrice, removeItem, isLoading } = useCart()
  const router = useRouter()

  useEffect(() => {
    if (!authLoading && !user) router.push('/login')
  }, [user, authLoading, router])

  if (authLoading || isLoading) return <div className="flex justify-center py-40"><LoadingSpinner size="lg" /></div>
  if (!user) return null

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 py-12 lg:py-20 flex flex-col md:flex-row gap-12 items-start">
      {/* Left: Cart Items */}
      <div className="flex-1 w-full">
        <h1 className="text-4xl font-black text-gray-900 mb-8 tracking-tight">Shopping Cart</h1>

        {items.length === 0 ? (
          <EmptyState icon="🛒" title="Your cart is empty" message="Browse our top-tier products to get started." action={{ label: 'Shop Now', href: '/' }} />
        ) : (
          <div className="flex flex-col gap-6">
            {items.map((item: any) => (
              <div key={item.id} className="flex items-center gap-6 p-6 rounded-2xl bg-white border border-gray-200 shadow-sm">
                <div className="w-24 h-24 rounded-xl bg-gray-50 flex items-center justify-center overflow-hidden flex-shrink-0 p-2 border border-gray-100 relative">
                  {item.productImageUrl ? (
                    <Image
                      src={getFullImageUrl(item.productImageUrl)}
                      alt={item.productName}
                      fill
                      sizes="96px"
                      className="object-contain mix-blend-multiply"
                      unoptimized={getFullImageUrl(item.productImageUrl).startsWith('http://localhost')}
                    />
                  ) : (
                    <span className="text-3xl">📦</span>
                  )}
                </div>
                <div className="flex-1 min-w-0">
                  <Link href={item.productSlug ? `/products/${item.productSlug}` : `/`} className="font-bold text-gray-900 text-lg line-clamp-1 hover:text-indigo-600 transition-colors">
                    {item.productName}
                  </Link>
                  <p className="text-sm font-medium text-gray-500 mt-1">Quantity: {item.quantity} × {formatPrice(item.unitPrice)}</p>
                </div>
                <div className="text-right flex flex-col items-end">
                  <p className="font-black text-xl text-gray-900">{formatPrice(item.subtotal)}</p>
                  <button onClick={() => removeItem(item.id)} className="text-sm font-bold text-red-500 hover:text-red-700 mt-2 transition-colors underline underline-offset-2">Remove</button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Right: Summary Sidebar */}
      {items.length > 0 && (
        <div className="w-full md:w-96 rounded-2xl bg-gray-50 border border-gray-200 p-8 sticky top-24">
          <h2 className="text-2xl font-black text-gray-900 mb-6 pb-4 border-b border-gray-200">Order Summary</h2>
          
          <div className="flex justify-between items-center mb-4 text-gray-600 font-medium">
            <span>Subtotal</span>
            <span className="font-bold text-gray-900">{formatPrice(totalPrice)}</span>
          </div>
          <div className="flex justify-between items-center mb-4 text-gray-600 font-medium">
            <span>Estimated Shipping</span>
            <span className="font-bold text-gray-900">Free</span>
          </div>
          <div className="flex justify-between items-center mb-6 text-gray-600 font-medium pb-6 border-b border-gray-200">
            <span>Estimated Taxes</span>
            <span className="font-bold text-gray-900">Calculated at checkout</span>
          </div>

          <div className="flex justify-between items-center mb-8">
            <span className="text-xl font-bold text-gray-900">Total</span>
            <span className="text-3xl font-black text-gray-900">{formatPrice(totalPrice)}</span>
          </div>
          
          <button 
            onClick={() => router.push('/checkout')} 
            className="w-full bg-indigo-600 hover:bg-indigo-700 text-white rounded-full py-4 text-base font-bold shadow-lg shadow-indigo-200 transition-all active:scale-[0.98]"
          >
            Checkout Securely
          </button>
        </div>
      )}
    </div>
  )
}
