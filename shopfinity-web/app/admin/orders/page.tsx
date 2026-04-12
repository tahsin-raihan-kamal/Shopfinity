'use client'

import { useState } from 'react'
import { useAdminOrders, useUpdateOrderStatus } from '@/hooks/useOrders'
import { OrderResponseDto, ORDER_STATUS, ORDER_STATUS_COLOR, OrderStatus } from '@/types'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { EmptyState } from '@/components/EmptyState'
import toast from 'react-hot-toast'
import { formatPrice } from '@/lib/formatters'

export default function AdminOrdersPage() {
  const { data: orders = [], isLoading } = useAdminOrders()
  const statusMutation = useUpdateOrderStatus()
  const [updating, setUpdating]   = useState<string | null>(null)

  const handleStatusChange = async (orderId: string, status: OrderStatus) => {
    setUpdating(orderId)
    try {
      await statusMutation.mutateAsync({ id: orderId, dto: { status } })
      toast.success('Order status updated')
    } catch { toast.error('Failed to update status') }
    finally { setUpdating(null) }
  }

  if (isLoading) return <div className="flex justify-center py-40"><LoadingSpinner size="lg" /></div>

  return (
    <div className="p-8">
      <div className="flex items-end justify-between mb-8 pb-4 border-b border-gray-200">
        <div>
           <h1 className="text-3xl font-black text-gray-900 tracking-tight">Order Management</h1>
           <p className="text-gray-500 font-medium mt-1">Review and process customer orders.</p>
        </div>
        <span className="text-sm font-bold bg-indigo-50 text-indigo-700 px-3 py-1 rounded-full">{orders.length} total</span>
      </div>

      {orders.length === 0 ? (
        <EmptyState icon="📋" title="No orders yet" message="Orders will appear here when customers check out." />
      ) : (
        <div className="space-y-4">
          {orders.map(order => (
            <div key={order.id} className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm hover:shadow-md transition-shadow">
              <div className="flex flex-col md:flex-row items-start md:items-center justify-between gap-4 mb-4">
                <div>
                  <div className="flex items-center gap-2 mb-1">
                     <span className="font-bold text-gray-900">Order ID:</span>
                     <p className="text-xs bg-gray-100 text-gray-600 font-mono px-2 py-0.5 rounded-md truncate max-w-[200px]">{order.id}</p>
                  </div>
                  <p className="text-sm text-gray-500 font-medium">{new Date(order.createdAt).toLocaleString()}</p>
                </div>
                <div className="flex items-center gap-6">
                  <div className="flex flex-col items-end">
                     <span className="text-xs text-gray-400 font-bold uppercase tracking-wider">Total</span>
                     <span className="text-2xl font-black text-gray-900">{formatPrice(order.totalAmount)}</span>
                  </div>

                  {/* Status Dropdown */}
                  <div className="relative">
                    <select
                      value={order.status}
                      onChange={e => handleStatusChange(order.id, e.target.value as OrderStatus)}
                      disabled={updating === order.id}
                      className={`appearance-none rounded-xl px-4 py-2 text-sm font-bold border cursor-pointer focus:outline-none focus:ring-2 focus:ring-indigo-500 shadow-sm disabled:opacity-50 transition-colors ${ORDER_STATUS_COLOR[order.status]}`}
                    >
                      {Object.values(ORDER_STATUS).map(s => (
                        <option key={s} value={s} className="bg-white text-gray-900 font-medium">
                          {s}
                        </option>
                      ))}
                    </select>
                    {updating === order.id && (
                      <div className="absolute inset-0 flex items-center justify-center bg-white/50 rounded-xl">
                        <div className="h-4 w-4 animate-spin rounded-full border-2 border-indigo-200 border-t-indigo-600" />
                      </div>
                    )}
                  </div>
                </div>
              </div>

              {/* Items */}
              <div className="space-y-2 border-t border-gray-100 pt-4 bg-gray-50/50 p-4 rounded-xl">
                {order.items.map((item, i) => (
                  <div key={i} className="flex justify-between items-center text-sm">
                    <div className="flex items-center gap-2">
                       <span className="text-gray-900 font-bold">{item.productName}</span> 
                       <span className="text-gray-500 text-xs bg-gray-200 px-1.5 py-0.5 rounded-md font-bold">Qty: {item.quantity}</span>
                    </div>
                    <span className="font-bold text-gray-600">{formatPrice(item.unitPrice * item.quantity)}</span>
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
