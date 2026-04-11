'use client'

import { useCallback, useEffect, useRef, useState } from 'react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useDebounce } from '@/hooks/useDebounce'
import { useSearchSuggestions } from '@/hooks/useSearch'
import { formatPrice } from '@/lib/formatters'
import { getFullImageUrl } from '@/lib/utils'
import Image from 'next/image'
import type { ProductSearchSuggestionDto } from '@/types'

export function NavbarSearch() {
  const router = useRouter()
  const [term, setTerm] = useState('')
  const [open, setOpen] = useState(false)
  const [active, setActive] = useState(-1)
  const debounced = useDebounce(term, 300)
  const { data: suggestions = [], isFetching, isLoading } = useSearchSuggestions(debounced)

  const wrapRef = useRef<HTMLDivElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)

  const showPanel = open && debounced.trim().length >= 1

  useEffect(() => {
    setActive(-1)
  }, [suggestions, debounced])

  useEffect(() => {
    const onDoc = (e: MouseEvent) => {
      if (!wrapRef.current?.contains(e.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', onDoc)
    return () => document.removeEventListener('mousedown', onDoc)
  }, [])

  const goProduct = useCallback(
    (s: ProductSearchSuggestionDto) => {
      setOpen(false)
      setTerm('')
      router.push(`/products/${s.slug}`)
    },
    [router]
  )

  const submitSearch = useCallback(() => {
    const q = term.trim()
    if (!q) return
    if (active >= 0 && suggestions[active]) {
      goProduct(suggestions[active])
      return
    }
    setOpen(false)
    router.push(`/?search=${encodeURIComponent(q)}`)
  }, [term, active, suggestions, router, goProduct])

  const loading = isFetching || (isLoading && debounced.trim().length >= 1)

  const onKeyDown = (e: React.KeyboardEvent) => {
    if (!showPanel && e.key === 'ArrowDown' && term.trim()) {
      setOpen(true)
      return
    }
    if (!showPanel) return

    if (e.key === 'ArrowDown') {
      e.preventDefault()
      setActive(i => Math.min(i + 1, Math.max(0, suggestions.length - 1)))
    } else if (e.key === 'ArrowUp') {
      e.preventDefault()
      setActive(i => Math.max(i - 1, 0))
    } else if (e.key === 'Enter') {
      e.preventDefault()
      submitSearch()
    } else if (e.key === 'Escape') {
      setOpen(false)
    }
  }

  return (
    <div ref={wrapRef} className="relative flex-1 min-w-0 max-w-md">
      <form
        className="relative"
        onSubmit={e => {
          e.preventDefault()
          submitSearch()
        }}
      >
        <span className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">
          {loading ? (
            <span className="inline-block h-4 w-4 animate-spin rounded-full border-2 border-gray-300 border-t-indigo-600" />
          ) : (
            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          )}
        </span>
        <input
          ref={inputRef}
          id="navbar-search"
          type="search"
          autoComplete="off"
          placeholder="Search products…"
          value={term}
          onChange={e => {
            setTerm(e.target.value)
            setOpen(true)
          }}
          onFocus={() => setOpen(true)}
          onKeyDown={onKeyDown}
          role="combobox"
          aria-expanded={showPanel}
          aria-controls="navbar-search-listbox"
          aria-activedescendant={active >= 0 ? `search-opt-${active}` : undefined}
          className="w-full rounded-xl border border-gray-200 bg-gray-50 py-2.5 pl-10 pr-3 text-sm font-medium text-gray-900 placeholder:text-gray-400 focus:border-indigo-500 focus:bg-white focus:outline-none focus:ring-2 focus:ring-indigo-500/30"
        />
      </form>

      {showPanel && (
        <div
          id="navbar-search-listbox"
          role="listbox"
          className="absolute left-0 right-0 top-full z-50 mt-2 max-h-80 overflow-y-auto rounded-xl border border-gray-100 bg-white py-2 shadow-xl"
        >
          {loading && suggestions.length === 0 && (
            <div className="px-4 py-6 text-center text-sm font-medium text-gray-500">Searching…</div>
          )}
          {!loading && suggestions.length === 0 && (
            <div className="px-4 py-6 text-center text-sm font-medium text-gray-500">No matches</div>
          )}
          {suggestions.map((s, idx) => {
            const img = getFullImageUrl(s.imageUrl)
            const selected = idx === active
            return (
              <Link
                key={s.id}
                id={`search-opt-${idx}`}
                role="option"
                aria-selected={selected}
                href={`/products/${s.slug}`}
                className={`flex items-center gap-3 px-3 py-2.5 text-sm ${selected ? 'bg-indigo-50' : 'hover:bg-gray-50'}`}
                onMouseEnter={() => setActive(idx)}
                onClick={() => {
                  setOpen(false)
                  setTerm('')
                }}
              >
                <div className="relative h-10 w-10 flex-shrink-0 overflow-hidden rounded-lg border border-gray-100 bg-gray-50">
                  {img ? (
                    <Image
                      src={img}
                      alt=""
                      fill
                      className="object-contain p-1"
                      sizes="40px"
                      unoptimized={img.startsWith('http://localhost') || img.startsWith('http://127.0.0.1')}
                    />
                  ) : (
                    <span className="flex h-full items-center justify-center text-lg opacity-40">📦</span>
                  )}
                </div>
                <div className="min-w-0 flex-1">
                  <div className="truncate font-bold text-gray-900">{s.name}</div>
                  <div className="text-xs font-semibold text-gray-500">{formatPrice(s.price)}</div>
                </div>
              </Link>
            )
          })}
        </div>
      )}
    </div>
  )
}
