'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { useAuth } from '@/context/AuthContext'
import { useCart } from '@/hooks/useCart'
import { useCategories } from '@/hooks/useCategories'
import { NavbarSearch } from '@/components/NavbarSearch'

export function Navbar() {
  const { user, isAdmin, logout } = useAuth()
  const { itemCount } = useCart()
  const pathname = usePathname()
  const { data: categories = [] } = useCategories()

  const navLink = (href: string, label: string) => (
    <Link
      href={href}
      className={`text-sm font-semibold transition-colors ${
        pathname === href ? 'text-indigo-600 border-b-2 border-indigo-600 pb-1' : 'text-gray-600 hover:text-black py-1'
      }`}
    >
      {label}
    </Link>
  )

  return (
    <nav className="fixed top-0 left-0 right-0 z-50 bg-white border-b border-gray-200 shadow-sm h-16 flex items-center">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 w-full flex items-center gap-3 md:gap-4">
        <div className="flex items-center gap-4 lg:gap-8 flex-shrink-0">
          <Link href="/" className="text-xl sm:text-2xl font-black text-gray-900 tracking-tight whitespace-nowrap">
            Shopfinity
          </Link>
          <div className="hidden md:flex items-center gap-6 mt-1 group relative">
            <span className="text-sm font-semibold text-gray-600 hover:text-black py-1 cursor-default">Categories ▾</span>

            <div className="absolute top-full left-0 pt-4 w-96 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200">
              <div className="bg-white rounded-2xl shadow-xl border border-gray-100 p-6 grid grid-cols-2 gap-x-8 gap-y-4">
                {categories.map((c: { id: string; name: string; slug: string }) => (
                  <Link
                    key={c.id}
                    href={`/?categorySlug=${encodeURIComponent(c.slug)}`}
                    className="text-sm text-gray-600 hover:text-indigo-600 hover:font-bold transition-colors"
                  >
                    {c.name}
                  </Link>
                ))}
              </div>
            </div>

            {navLink('/', 'All Products')}
            {isAdmin && navLink('/admin/dashboard', 'Admin Panel')}
          </div>
        </div>

        <NavbarSearch />

        <div className="flex items-center gap-3 sm:gap-4 flex-shrink-0">
          {user && (
            <Link href="/cart" className="relative group text-gray-600 hover:text-black flex items-center gap-2 transition-colors">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z"
                />
              </svg>
              <span className="hidden sm:block text-sm font-semibold">Cart</span>
              {itemCount > 0 && (
                <span className="absolute -top-1.5 -right-2 sm:right-6 h-5 w-5 flex items-center justify-center rounded-full bg-red-600 text-white text-xs font-bold ring-2 ring-white">
                  {itemCount > 9 ? '9+' : itemCount}
                </span>
              )}
            </Link>
          )}

          {user ? (
            <div className="flex items-center gap-3 sm:gap-4">
              <Link href="/wishlist" className="text-gray-600 hover:text-black transition-colors" title="My Wishlist">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                </svg>
              </Link>
              <Link href="/orders" className="text-sm font-semibold text-gray-600 hover:text-black transition-colors hidden sm:block">
                Orders
              </Link>
              <button
                type="button"
                onClick={logout}
                className="text-sm font-semibold text-gray-600 hover:text-red-600 transition-colors hidden sm:block"
              >
                Logout
              </button>
            </div>
          ) : (
            <div className="flex items-center gap-3 sm:gap-4">
              <Link href="/login" className="text-sm font-semibold text-gray-600 hover:text-black transition-colors">
                Sign In
              </Link>
              <Link
                href="/register"
                className="px-3 sm:px-4 py-2 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-black tracking-wide shadow-sm transition-colors whitespace-nowrap"
              >
                Sign Up
              </Link>
            </div>
          )}
        </div>
      </div>
    </nav>
  )
}
