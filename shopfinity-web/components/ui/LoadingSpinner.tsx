interface LoadingSpinnerProps {
  fullScreen?: boolean
  size?: 'sm' | 'md' | 'lg'
}

const sizes = { sm: 'h-5 w-5', md: 'h-8 w-8', lg: 'h-12 w-12' }

export function LoadingSpinner({ fullScreen, size = 'md' }: LoadingSpinnerProps) {
  const spinner = (
    <div className="flex flex-col items-center gap-3">
      <div className={`${sizes[size]} animate-spin rounded-full border-2 border-white/10 border-t-indigo-500`} />
      {fullScreen && <p className="text-sm text-gray-400 animate-pulse">Loading…</p>}
    </div>
  )

  if (fullScreen) {
    return (
      <div className="fixed inset-0 flex items-center justify-center bg-[#0a0a0f]/80 backdrop-blur-sm z-50">
        {spinner}
      </div>
    )
  }
  return spinner
}
