'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'

const links = [
  { href: '/admin/dashboard',  label: 'Dashboard',  icon: '📊' },
  { href: '/admin/products',   label: 'Products',   icon: '📦' },
  { href: '/admin/categories', label: 'Categories', icon: '🏷️' },
  { href: '/admin/orders',     label: 'Orders',     icon: '📋' },
]

export function AdminSidebar() {
  const pathname = usePathname()
  return (
    <aside className="w-64 min-h-screen border-r border-gray-200 bg-white flex flex-col fixed md:sticky top-0 h-screen z-40">
      <div className="px-6 py-6 border-b border-gray-100 flex items-center justify-between">
        <div>
          <Link href="/" className="text-xl font-black text-gray-900 tracking-tight">
            Shopfinity
          </Link>
          <p className="text-xs font-bold text-gray-400 mt-1 uppercase tracking-widest">Admin</p>
        </div>
      </div>
      <nav className="flex-1 px-4 py-6 space-y-2 overflow-y-auto">
        <p className="text-xs font-bold text-gray-400 mb-4 px-2 uppercase tracking-wider">Main Menu</p>
        {links.map(({ href, label, icon }) => (
          <Link
            key={href}
            href={href}
            className={`flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold transition-all ${
              pathname.startsWith(href)
                ? 'bg-indigo-50 text-indigo-700'
                : 'text-gray-600 hover:text-gray-900 hover:bg-gray-50'
            }`}
          >
            <span className="text-lg w-6 text-center">{icon}</span>{label}
          </Link>
        ))}
      </nav>
      <div className="p-4 border-t border-gray-100">
        <Link href="/" className="flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold text-gray-500 hover:text-gray-900 hover:bg-gray-50 transition-all">
          <span className="text-lg">🏕</span> Back to Storefront
        </Link>
      </div>
    </aside>
  )
}
