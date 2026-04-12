'use client'

import { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { AuthUser } from '@/types'

interface AuthContextType {
  user: AuthUser | null
  isLoading: boolean
  isAdmin: boolean
  login: (email: string, roles: string[]) => void
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextType | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser]       = useState<AuthUser | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  // 1. Initial Handshake: initialize CSRF token and hydrate session
  useEffect(() => {
    // Fire-and-forget CSRF handshake
    const baseUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5049';
    fetch(`${baseUrl}/api/v1/auth/csrf`, { method: 'GET', credentials: 'include' })
      .catch(() => {/* ignore initial handshake errors, will fail later if critical */})

    const stored = sessionStorage.getItem('shopfinity_user')
    if (stored) {
      try {
        const userData = JSON.parse(stored)
        setUser(userData)
      } catch { /* ignore */ }
    }
    setIsLoading(false)
  }, [])

  const login = (email: string, roles: string[]) => {
    const userData = { email, roles }
    setUser(userData)
    sessionStorage.setItem('shopfinity_user', JSON.stringify(userData))
  }

  const logout = async () => {
    await fetch('/api/auth/logout', { method: 'POST', credentials: 'include' })
    setUser(null)
    sessionStorage.removeItem('shopfinity_user')
    window.location.href = '/login'
  }

  return (
    <AuthContext.Provider value={{
      user,
      isLoading,
      isAdmin: user?.roles.includes('Admin') ?? false,
      login,
      logout,
    }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
