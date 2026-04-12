'use client'

import { useEffect } from 'react'
import { useAuth } from '@/context/AuthContext'
import { useOrders } from '@/hooks/useOrders'
import { OrderResponseDto, ORDER_STATUS_COLOR } from '@/types'
import { EmptyState } from '@/components/EmptyState'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { useRouter } from 'next/navigation'
import { formatPrice } from '@/lib/formatters'

export default function OrdersPage() {
  const { user, isLoading: authLoading } = useAuth()
  const router = useRouter()

  const { data: orders = [], isLoading } = useOrders()

  useEffect(() => {
    if (!authLoading && !user) router.push('/login')
  }, [user, authLoading, router])

  if (authLoading || isLoading) return <div className="flex justify-center py-20"><LoadingSpinner size="lg" /></div>
  if (!user) return null

  return (
    <div className="max-w-4xl mx-auto px-4 py-10">
      <h1 className="text-3xl font-bold text-white mb-8">My Orders</h1>

      {orders.length === 0 ? (
        <EmptyState icon="📋" title="No orders yet" message="Place your first order to see it here." action={{ label: 'Shop Now', href: '/' }} />
      ) : (
        <div className="space-y-4">
          {orders.map((order: OrderResponseDto) => (
            <div key={order.id} className="rounded-2xl border border-white/10 bg-white/5 p-5">
              <div className="flex items-center justify-between mb-4">
                <div>
                  <p className="text-xs text-gray-500 font-mono">{order.id}</p>
                  <p className="text-sm text-gray-400">{new Date(order.createdAt).toLocaleDateString()}</p>
                </div>
                <div className="flex items-center gap-3">
                  <span className={`inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold border ${ORDER_STATUS_COLOR[order.status]}`}>
                    {order.status}
                  </span>
                  <span className="text-indigo-600 font-black text-xl">{formatPrice(order.totalAmount)}</span>
                </div>
              </div>
              <div className="space-y-1 border-t border-white/10 pt-3">
                {order.items.map((item: any, i: number) => (
                  <div key={i} className="flex justify-between text-sm">
                    <span className="text-gray-300">{item.productName} <span className="text-gray-500">×{item.quantity}</span></span>
                    <span className="text-gray-600 font-bold">{formatPrice(item.unitPrice * item.quantity)}</span>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
