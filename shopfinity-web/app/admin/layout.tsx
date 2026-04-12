'use client'

import { useAuth } from '@/context/AuthContext'
import { AdminSidebar } from '@/components/AdminSidebar'
import { useRouter } from 'next/navigation'
import { useEffect } from 'react'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  const { user, isAdmin, isLoading } = useAuth()
  const router = useRouter()

  useEffect(() => {
    if (!isLoading && (!user || !isAdmin)) router.push('/')
  }, [user, isAdmin, isLoading, router])

  if (isLoading) return <LoadingSpinner fullScreen />
  if (!user || !isAdmin) return null

  return (
    <div className="flex min-h-[calc(100vh-4rem)]">
      <AdminSidebar />
      <div className="flex-1 overflow-auto">
        <div className="max-w-6xl mx-auto px-6 py-8">{children}</div>
      </div>
    </div>
  )
}
