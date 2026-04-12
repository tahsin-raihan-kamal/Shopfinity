import Link from 'next/link'
import Image from 'next/image'
import { ProductDto } from '@/types'
import { formatPrice } from '@/lib/formatters'

import { getFullImageUrl } from '@/lib/utils'

interface ProductCardProps {
  product: ProductDto
  onAddToCart?: () => void
}

export function ProductCard({ product, onAddToCart }: ProductCardProps) {
  const imageUrl = getFullImageUrl(product.imageUrl)

  return (
    <div className="group relative flex flex-col rounded-xl bg-white border border-gray-100 overflow-hidden hover:shadow-lg hover:-translate-y-0.5 transition-all duration-300">
      {/* Image */}
      <Link href={`/products/${product.slug}`}>
        <div className="relative h-64 bg-gray-50 overflow-hidden flex items-center justify-center p-4">
          {imageUrl ? (
            <Image
              src={imageUrl}
              alt={product.name}
              fill
              sizes="(max-width: 768px) 100vw, 33vw"
              className="object-contain group-hover:scale-105 transition-transform duration-500 mix-blend-multiply p-4"
              unoptimized={imageUrl.startsWith('http://localhost') || imageUrl.startsWith('http://127.0.0.1')}
            />
          ) : (
            <div className="flex items-center justify-center h-full text-5xl opacity-30">📦</div>
          )}
          {product.stockQuantity === 0 && (
            <div className="absolute inset-0 bg-white/70 backdrop-blur-sm flex items-center justify-center">
              <span className="text-xs font-bold tracking-widest uppercase text-red-600 bg-red-50 px-3 py-1 rounded-full border border-red-200">Out of Stock</span>
            </div>
          )}
        </div>
      </Link>

      {/* Info */}
      <div className="flex flex-col flex-1 p-5 gap-2 border-t border-gray-50">
        <div className="flex justify-between items-start gap-2">
          <Link href={`/products/${product.slug}`} className="flex-1">
            <h3 className="font-bold text-gray-900 text-sm line-clamp-2 hover:text-indigo-600 transition-colors leading-snug">{product.name}</h3>
          </Link>
          <span className="text-base font-black text-gray-900">{formatPrice(product.price)}</span>
        </div>
        <p className="text-xs text-gray-500 line-clamp-2 mt-1">{product.description}</p>
        
        <div className="mt-4 pt-4 border-t border-gray-100">
          <button
            onClick={onAddToCart}
            disabled={product.stockQuantity === 0}
            className="w-full py-2.5 rounded-lg bg-gray-900 hover:bg-black text-white text-sm font-bold transition-all disabled:opacity-40 disabled:cursor-not-allowed flex items-center justify-center gap-2 group-hover:bg-indigo-600"
          >
            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
               <path strokeLinecap="round" strokeLinejoin="round" d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
            </svg>
            Add to Cart
          </button>
        </div>
      </div>
    </div>
  )
}
