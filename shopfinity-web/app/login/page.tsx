'use client'

import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { loginSchema, LoginInput } from '@/lib/schemas'
import { authService } from '@/services/authService'
import { useAuth } from '@/context/AuthContext'
import { useRouter } from 'next/navigation'
import { Input } from '@/components/ui/Input'
import { Button } from '@/components/ui/Button'
import Link from 'next/link'
import toast from 'react-hot-toast'
import type { Metadata } from 'next'

export default function LoginPage() {
  const { login } = useAuth()
  const router    = useRouter()

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<LoginInput>({
    resolver: zodResolver(loginSchema),
  })

  const onSubmit = async (data: LoginInput) => {
    try {
      const res = await authService.login(data)
      login(res.email, res.roles)
      toast.success('Welcome back!')
      router.push(res.roles.includes('Admin') ? '/admin/dashboard' : '/')
    } catch (e: unknown) {
      toast.error(e instanceof Error ? e.message : 'Login failed')
    }
  }

  return (
    <div className="min-h-[calc(100vh-4rem)] flex items-center justify-center px-4">
      <div className="w-full max-w-md">
        {/* Card */}
        <div className="rounded-2xl border border-white/10 bg-white/5 backdrop-blur-xl p-8 shadow-2xl">
          <div className="text-center mb-8">
            <div className="text-4xl mb-3">🔐</div>
            <h1 className="text-2xl font-bold text-white">Welcome back</h1>
            <p className="text-gray-300 text-sm mt-1">Sign in to your Shopfinity account</p>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
            <Input id="email" label="Email" type="email" placeholder="you@example.com"
              {...register('email')} error={errors.email?.message} />
            <Input id="password" label="Password" type="password" placeholder="••••••••"
              {...register('password')} error={errors.password?.message} />

            <Button type="submit" fullWidth isLoading={isSubmitting}>
              {isSubmitting ? 'Signing in…' : 'Sign In'}
            </Button>
          </form>

          <p className="text-center text-sm text-gray-500 mt-6">
            Don&apos;t have an account?{' '}
            <Link href="/register" className="text-indigo-400 hover:text-indigo-300 font-medium">Sign up</Link>
          </p>
        </div>
      </div>
    </div>
  )
}
