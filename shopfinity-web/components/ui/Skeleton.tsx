export function Skeleton({ className = '' }: { className?: string }) {
  return (
    <div
      className={`animate-pulse bg-gray-200/50 rounded-md ${className}`}
      aria-hidden="true"
    />
  )
}

export function ProductCardSkeleton() {
  return (
    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
      <Skeleton className="w-full h-64 rounded-none bg-gray-100" />
      <div className="p-6">
        <Skeleton className="h-4 w-1/4 mb-3" />
        <Skeleton className="h-6 w-3/4 mb-4" />
        <div className="space-y-2 mb-6">
          <Skeleton className="h-3 w-full" />
          <Skeleton className="h-3 w-5/6" />
        </div>
        <div className="flex items-center justify-between">
          <Skeleton className="h-8 w-24" />
          <Skeleton className="h-8 w-8 rounded-full" />
        </div>
      </div>
    </div>
  )
}

export function ImageSkeleton({ className = '' }: { className?: string }) {
  return <Skeleton className={`w-full h-full bg-gray-100 ${className}`} />
}
