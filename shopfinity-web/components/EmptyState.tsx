import Link from 'next/link'

interface EmptyStateProps {
  icon?: string
  title: string
  message?: string
  action?: { label: string; href: string }
}

export function EmptyState({ icon = '📭', title, message, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-20 text-center">
      <div className="text-6xl mb-4 opacity-50">{icon}</div>
      <h3 className="text-xl font-bold text-gray-900 mb-2">{title}</h3>
      {message && <p className="text-gray-500 max-w-sm mb-6">{message}</p>}
      {action && (
        <Link
          href={action.href}
          className="px-6 py-3 rounded-lg bg-gray-900 hover:bg-black text-white text-sm font-bold transition-colors"
        >
          {action.label}
        </Link>
      )}
    </div>
  )
}
