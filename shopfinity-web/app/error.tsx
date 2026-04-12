'use client'
 
import { useEffect } from 'react'
 
export default function Error({
  error,
  reset,
}: {
  error: Error & { digest?: string }
  reset: () => void
}) {
  useEffect(() => {
    // Log the error to an error reporting service
    console.error(error)
  }, [error])
 
  return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] px-6 text-center">
      <div className="w-24 h-24 bg-red-100 rounded-full flex items-center justify-center mb-6">
        <span className="text-4xl">⚠️</span>
      </div>
      <h2 className="text-3xl font-black text-gray-900 mb-4">Something went wrong!</h2>
      <p className="text-gray-600 max-w-md mx-auto mb-8">
        We hit a snag trying to load this page. This could be a temporary issue or a broken link.
      </p>
      <button
        onClick={() => reset()}
        className="px-8 py-3 bg-gray-900 text-white font-bold rounded-lg hover:bg-gray-800 transition-colors shadow-lg"
      >
        Try again
      </button>
    </div>
  )
}
