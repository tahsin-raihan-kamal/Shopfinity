'use client'

import { useState, use } from 'react'
import { ProductDto } from '@/types'
import { useCart } from '@/hooks/useCart'
import { useAuth } from '@/context/AuthContext'
import { useProduct } from '@/hooks/useProducts'
import { useWishlist, useAddWishlistItem, useRemoveWishlistItem } from '@/hooks/useWishlist'
import { useProductReviews, useAddReview } from '@/hooks/useReviews'
import { Button } from '@/components/ui/Button'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import toast from 'react-hot-toast'
import { formatPrice } from '@/lib/formatters'
import Image from 'next/image'
import { getFullImageUrl } from '@/lib/utils'

export default function ProductDetailClient(props: { params: Promise<{ slug: string }> }) {
  const params              = use(props.params)
  const { user }            = useAuth()
  const { addItem }         = useCart()
  
  const [qty, setQty]           = useState(1)
  const [adding, setAdding]     = useState(false)
  const [reviewTab, setRevTab]  = useState(false)

  // React Query Hooks
  const { data: product, isLoading: loading } = useProduct(params.slug)
  const imageUrl = getFullImageUrl(product?.imageUrl)
  
  const { data: wishlistItems } = useWishlist()
  
  const addWishlistMut = useAddWishlistItem()
  const rmWishlistMut = useRemoveWishlistItem()

  // Reviews
  const { data: reviews = [], isLoading: loadingReviews, isError: errorReviews } = useProductReviews(product?.id ?? '')
  const addReviewMut = useAddReview()
  
  const [rating, setRating] = useState(5)
  const [title, setTitle] = useState('')
  const [comment, setComment] = useState('')
  const [visibleReviewsCount, setVisibleReviewsCount] = useState(5)
  
  if (loading) return <div className="flex justify-center py-40"><LoadingSpinner size="lg" /></div>
  if (!product) return <div className="py-40 text-center font-bold text-2xl text-gray-400">Product not found.</div>

  const isWishlistedItem = wishlistItems?.find(w => w.productId === product.id)
  const isWishlisted = !!isWishlistedItem

  const handleAdd = async () => {
    if (!user) { toast.error('Please sign in first'); return }
    setAdding(true)
    await addItem({ productId: product.id, quantity: qty })
    setAdding(false)
    toast.success('Added to Cart')
  }

  const toggleWishlist = async () => {
    if (!user) { toast.error('Please sign in to wishlist items'); return }
    try {
      if (isWishlisted && isWishlistedItem) {
        await rmWishlistMut.mutateAsync(isWishlistedItem.id)
        toast.success('Removed from Wishlist')
      } else {
        await addWishlistMut.mutateAsync({ productId: product.id })
        toast.success('Added to Wishlist')
      }
    } catch {
      // 409 / 404 / network: axios interceptor shows message
    }
  }

  const submitReview = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!user) { toast.error('Please sign in to review'); return }
    if (!title.trim() || !comment.trim()) { toast.error('Please fill out the review form fully'); return; }
    
    try {
      await addReviewMut.mutateAsync({
        productId: product.id,
        rating,
        title,
        comment
      })
      setTitle(''); setComment(''); setRating(5);
    } catch (e) {
      // Handled by mutation onError toast
    }
  }

  const avgRating = reviews.length > 0 
    ? (reviews.reduce((acc, r) => acc + r.rating, 0) / reviews.length).toFixed(1)
    : '0'

  const userAlreadyReviewed = user && reviews.some(r => r.userId === user.email || r.userName === user.email);
  const visibleReviews = reviews.slice(0, visibleReviewsCount);

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 py-12 lg:py-20 flex flex-col lg:flex-row gap-16">
      {/* Left: Sticky Image Gallery */}
      <div className="lg:w-1/2 flex flex-col gap-4">
        <div className="sticky top-24 rounded-2xl bg-gray-50 flex items-center justify-center border border-gray-200 overflow-hidden min-h-[500px] p-8 relative">
          {imageUrl
            ? (
              <Image
                src={imageUrl}
                alt={product.name}
                fill
                sizes="(max-width: 1024px) 100vw, 50vw"
                className="object-contain mix-blend-multiply p-8"
                priority
                unoptimized={imageUrl.startsWith('http://localhost') || imageUrl.startsWith('http://127.0.0.1')}
              />
            )
            : <span className="text-8xl opacity-10">📦</span>}
            
          {/* Trust Indicators Overlay */}
          <div className="absolute bottom-6 left-6 flex gap-2">
            <span className="bg-white/80 backdrop-blur-md border border-gray-200 text-gray-800 text-xs font-black uppercase tracking-wider px-3 py-1.5 rounded-full shadow-sm">Verified Authentic</span>
            <span className="bg-white/80 backdrop-blur-md border border-gray-200 text-gray-800 text-xs font-black uppercase tracking-wider px-3 py-1.5 rounded-full shadow-sm">Top Rated</span>
          </div>
        </div>
      </div>

      {/* Right: Details & Actions */}
      <div className="lg:w-1/2 flex flex-col">
        <nav className="text-sm text-gray-500 mb-6 font-medium">Home / Products / <span className="text-gray-900">{product.name}</span></nav>
        
        <h1 className="text-4xl lg:text-5xl font-black text-gray-900 leading-tight mb-4 tracking-tight">{product.name}</h1>
        
        {/* Star aggregate rendering */}
        <div className="flex items-center gap-2 mb-4">
          <div className="flex text-yellow-400 text-lg">
             {'★'.repeat(Math.round(Number(avgRating)))}{'☆'.repeat(5 - Math.round(Number(avgRating)))}
          </div>
          <span className="font-bold text-gray-900">{avgRating} Stars</span>
          <span className="text-gray-400 font-medium">({reviews.length} reviews)</span>
        </div>
        
        <div className="text-3xl font-black text-gray-900 mb-6 border-b border-gray-200 pb-8">{formatPrice(product.price)}</div>

        <div className="mb-8">
          <h2 className="text-lg font-bold text-gray-900 mb-3">Product Overview</h2>
          <p className="text-gray-600 leading-relaxed max-w-prose text-lg">{product.description}</p>
        </div>

        <div className={`text-sm font-bold tracking-wide uppercase mb-6 ${product.stockQuantity > 0 ? 'text-green-600' : 'text-red-600'}`}>
          {product.stockQuantity > 0 ? `In Stock (${product.stockQuantity} available)` : 'Out of Stock'}
        </div>

        {product.stockQuantity > 0 && (
          <div className="flex flex-col sm:flex-row items-center gap-4 border-t border-b border-gray-200 py-8 my-4">
            <div className="flex items-center gap-4 bg-gray-100 rounded-full px-6 py-3 min-w-[140px] justify-between">
              <button onClick={() => setQty(q => Math.max(1, q - 1))} className="text-gray-600 hover:text-black font-bold text-xl transition-colors">−</button>
              <span className="text-gray-900 font-bold text-lg select-none w-8 text-center">{qty}</span>
              <button onClick={() => setQty(q => Math.min(product.stockQuantity, q + 1))} className="text-gray-600 hover:text-black font-bold text-xl transition-colors">+</button>
            </div>
            
            <button 
              onClick={handleAdd} 
              disabled={adding}
              className="w-full sm:flex-1 bg-indigo-600 hover:bg-indigo-700 text-white rounded-full py-4 text-base font-bold shadow-lg shadow-indigo-200 transition-all active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2 focus:ring-4 focus:ring-indigo-300 focus:outline-none outline-none"
            >
              {adding ? <LoadingSpinner /> : 'Add to Cart — ' + formatPrice(product.price * qty)}
            </button>
            
            <button 
              onClick={toggleWishlist}
              className={`h-14 w-14 rounded-full flex items-center justify-center transition-colors focus:ring-4 focus:ring-red-200 focus:outline-none ${isWishlisted ? 'bg-red-50 text-red-500 hover:bg-gray-100 hover:text-gray-400' : 'bg-gray-100 text-gray-400 hover:text-red-500 hover:bg-red-50'}`}
              aria-label={isWishlisted ? "Remove from wishlist" : "Add to wishlist"}
              aria-pressed={isWishlisted}
            >
              <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill={isWishlisted ? "currentColor" : "none"} viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
              </svg>
            </button>
          </div>
        )}

        {/* Accoridon mock */}
        <div className="flex flex-col mt-8 divide-y divide-gray-200 border-t border-b border-gray-200">
           <div className="py-6 flex justify-between items-center cursor-pointer group">
             <h3 className="font-bold text-gray-900 text-lg group-hover:text-indigo-600 transition-colors">Technical Specifications</h3>
             <span className="text-gray-400 font-bold">+</span>
           </div>
           
           <div className="py-6 flex justify-between items-center cursor-pointer group" onClick={() => setRevTab(!reviewTab)}>
             <h3 className="font-bold text-gray-900 text-lg group-hover:text-indigo-600 transition-colors">Customer Reviews ({reviews.length})</h3>
             <span className="text-gray-400 font-bold">{reviewTab ? '−' : '+'}</span>
           </div>
           
           {/* Reviews Expanded Section */}
           {reviewTab && (
             <div className="py-6 bg-gray-50/50 -mx-4 px-4 sm:mx-0 sm:px-0">
               {errorReviews ? (
                  <div className="p-4 bg-red-50 text-red-600 rounded-lg text-sm font-bold text-center border border-red-100">
                     Unable to load reviews. Please try again later.
                  </div>
               ) : loadingReviews ? (
                  <div className="flex justify-center p-8"><LoadingSpinner size="sm" /></div>
               ) : (
                  <div className="space-y-8">
                     {reviews.length === 0 ? (
                        <p className="text-gray-500 italic">No reviews yet. Be the first!</p>
                     ) : (
                        <div className="space-y-6 max-h-[600px] overflow-y-auto pr-4 custom-scrollbar">
                           {visibleReviews.map(r => (
                              <div key={r.id} className="border-b border-gray-200 pb-6 last:border-0 last:pb-0">
                                 <div className="flex items-center gap-2 mb-1">
                                    <div className="flex text-yellow-400 text-sm">
                                       {'★'.repeat(r.rating)}{'☆'.repeat(5 - r.rating)}
                                    </div>
                                    <span className="font-bold text-gray-900 text-sm">{r.title}</span>
                                 </div>
                                 <p className="text-gray-600 text-sm mb-2">{r.comment}</p>
                                 <div className="text-xs text-gray-400 font-medium">
                                    {r.userName} • {new Date(r.createdAt).toLocaleDateString()}
                                 </div>
                              </div>
                           ))}
                           
                           {visibleReviewsCount < reviews.length && (
                             <div className="pt-4 flex justify-center">
                               <button 
                                 onClick={() => setVisibleReviewsCount(prev => prev + 5)}
                                 className="px-6 py-2 rounded-full border-2 border-gray-200 text-sm font-bold text-gray-600 hover:border-gray-900 hover:text-gray-900 transition-colors"
                               >
                                 Load More Reviews
                               </button>
                             </div>
                           )}
                        </div>
                     )}
                     
                     {/* Write Review Form */}
                     {user && !userAlreadyReviewed && (
                        <form onSubmit={submitReview} className="bg-white p-6 rounded-xl border border-gray-200 shadow-sm mt-8">
                           <h4 className="font-black text-gray-900 mb-4 tracking-tight">Write a Review</h4>
                           <div className="flex items-center gap-2 mb-4">
                              <span className="text-sm font-bold text-gray-700">Rating</span>
                              <select value={rating} onChange={(e) => setRating(Number(e.target.value))} className="border border-gray-300 rounded px-2 py-1 text-sm font-medium">
                                 {[5,4,3,2,1].map(n => <option key={n} value={n}>{n} Stars</option>)}
                              </select>
                           </div>
                           <input type="text" placeholder="Review Title" value={title} onChange={(e) => setTitle(e.target.value)} required className="w-full mb-4 px-4 py-3 rounded-lg border border-gray-300 focus:ring-2 focus:ring-indigo-600 focus:outline-none text-sm font-medium" />
                           <textarea placeholder="Share your experience..." value={comment} onChange={(e) => setComment(e.target.value)} required rows={4} className="w-full mb-4 px-4 py-3 rounded-lg border border-gray-300 focus:ring-2 focus:ring-indigo-600 focus:outline-none text-sm" />
                           <Button type="submit" disabled={addReviewMut.isPending}>
                              {addReviewMut.isPending ? 'Submitting...' : 'Submit Review'}
                           </Button>
                        </form>
                     )}
                     {user && userAlreadyReviewed && (
                        <div className="bg-green-50 p-4 rounded-lg text-center text-sm font-bold text-green-700 mt-8 border border-green-100 flex items-center justify-center gap-2">
                           <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" /></svg>
                           You have already reviewed this product.
                        </div>
                     )}
                     {!user && (
                        <div className="bg-gray-100 p-4 rounded-lg text-center text-sm font-bold text-gray-500 mt-8">
                           Please sign in to write a review.
                        </div>
                     )}
                  </div>
               )}
             </div>
           )}
        </div>
      </div>
    </div>
  )
}
