import { EmptyState } from '@/components/EmptyState'
export default function NotFound() {
  return (
    <div className="min-h-screen flex items-center justify-center">
      <EmptyState icon="🔍" title="Page Not Found" message="The page you're looking for doesn't exist." action={{ label: 'Go Home', href: '/' }} />
    </div>
  )
}
