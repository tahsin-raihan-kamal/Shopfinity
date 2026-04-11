interface PaginationProps {
  currentPage: number
  totalPages: number
  onPageChange: (page: number) => void
}

export function Pagination({ currentPage, totalPages, onPageChange }: PaginationProps) {
  if (totalPages <= 1) return null

  const pages = Array.from({ length: totalPages }, (_, i) => i + 1)
  const visible = pages.filter(p => p === 1 || p === totalPages || Math.abs(p - currentPage) <= 1)

  return (
    <div className="flex items-center justify-center gap-2 mt-4">
      <button
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage === 1}
        className="px-4 py-2 rounded-lg bg-white border border-gray-200 text-gray-600 hover:text-gray-900 hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition-all font-semibold"
      >
        ← Prev
      </button>

      {visible.map((page, idx) => {
        const prev = visible[idx - 1]
        const showEllipsis = prev && page - prev > 1
        return (
          <div key={page} className="flex items-center gap-2">
            {showEllipsis && <span className="text-gray-400">…</span>}
            <button
              onClick={() => onPageChange(page)}
              className={`w-10 h-10 rounded-lg text-sm font-bold transition-all ${
                page === currentPage
                  ? 'bg-gray-900 text-white shadow-md'
                  : 'bg-white border border-gray-200 text-gray-600 hover:text-gray-900 hover:bg-gray-50'
              }`}
            >
              {page}
            </button>
          </div>
        )
      })}

      <button
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage === totalPages}
        className="px-4 py-2 rounded-lg bg-white border border-gray-200 text-gray-600 hover:text-gray-900 hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition-all font-semibold"
      >
        Next →
      </button>
    </div>
  )
}
