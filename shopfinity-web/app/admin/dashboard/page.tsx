'use client'

import { useDashboardMetrics } from '@/hooks/useDashboard'
import { formatPrice } from '@/lib/formatters'

interface Stat { label: string; value: number | string; icon: string; color: string }

export default function AdminDashboardPage() {
  const { data, isLoading } = useDashboardMetrics()

  const stats = [
    { label: 'Total Users',    value: data?.totalUsers ?? 0,    icon: '👥', color: 'from-blue-500 to-indigo-600 shadow-blue-500/30' },
    { label: 'Total Orders',   value: data?.totalOrders ?? 0,   icon: '📋', color: 'from-purple-500 to-pink-600 shadow-purple-500/30' },
    { label: 'Daily Sales',    value: formatPrice(data?.dailySales ?? 0), icon: '📈', color: 'from-orange-400 to-red-500 shadow-orange-500/30' },
    { label: 'Total Revenue',  value: formatPrice(data?.totalRevenue ?? 0), icon: '💰', color: 'from-emerald-400 to-teal-500 shadow-emerald-500/30' },
  ]

  return (
    <div className="p-8">
      <div className="mb-8">
         <h1 className="text-3xl font-black text-gray-900 tracking-tight">Dashboard Overview</h1>
         <p className="text-gray-500 mt-1 font-medium">Welcome back, Admin. Here&apos;s what&apos;s happening with your store today.</p>
      </div>

      {isLoading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          {Array.from({ length: 4 }).map((_, i) => (
            <div key={i} className="h-32 rounded-2xl bg-gray-200 animate-pulse" />
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          {stats.map(s => (
            <div key={s.label} className={`rounded-2xl bg-gradient-to-br ${s.color} p-6 shadow-lg flex items-center gap-4 hover:-translate-y-1 transition-transform`}>
              <div className="text-4xl bg-white/20 p-3 rounded-xl flex-shrink-0">{s.icon}</div>
              <div>
                <div className="text-3xl font-black text-white">{s.value}</div>
                <div className="text-sm font-semibold text-white/80 mt-1 uppercase tracking-wider">{s.label}</div>
              </div>
            </div>
          ))}
        </div>
      )}

      <div className="mt-12 rounded-2xl border border-gray-200 bg-white p-8 shadow-sm">
        <h2 className="text-xl font-bold text-gray-900 mb-6 border-b border-gray-100 pb-4">Quick Actions</h2>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
          {[
            { href: '/admin/products',   label: 'Manage Products',   desc: 'Add, edit or delete products', icon: '📦', bg: 'bg-indigo-50', text: 'text-indigo-700' },
            { href: '/admin/categories', label: 'Manage Categories', desc: 'Organize your store layout', icon: '🏷️', bg: 'bg-purple-50', text: 'text-purple-700' },
            { href: '/admin/orders',     label: 'View All Orders',   desc: 'Process and fulfill orders', icon: '📋', bg: 'bg-amber-50', text: 'text-amber-700' },
          ].map(l => (
            <a key={l.href} href={l.href} className="group flex flex-col p-6 rounded-2xl border border-gray-100 hover:border-gray-300 hover:shadow-md transition-all bg-white">
              <div className={`w-12 h-12 rounded-xl ${l.bg} ${l.text} flex items-center justify-center text-2xl mb-4 group-hover:scale-110 transition-transform`}>
                 {l.icon}
              </div>
              <h3 className="font-bold text-gray-900 text-lg">{l.label}</h3>
              <p className="text-sm text-gray-500 mt-1 font-medium">{l.desc}</p>
            </a>
          ))}
        </div>
      </div>
    </div>
  )
}
