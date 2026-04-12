'use client'

import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { registerSchema, RegisterInput } from '@/lib/schemas'
import { authService } from '@/services/authService'
import { useAuth } from '@/context/AuthContext'
import { useRouter } from 'next/navigation'
import { Input } from '@/components/ui/Input'
import { Button } from '@/components/ui/Button'
import Link from 'next/link'
import toast from 'react-hot-toast'

export default function RegisterPage() {
  const { login } = useAuth()
  const router    = useRouter()

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<RegisterInput>({
    resolver: zodResolver(registerSchema),
  })

  const onSubmit = async (data: RegisterInput) => {
    try {
      const res = await authService.register(data)
      login(res.email, res.roles)
      toast.success('Account created! Welcome to Shopfinity 🎉')
      router.push('/')
    } catch (e: unknown) {
      toast.error(e instanceof Error ? e.message : 'Registration failed')
    }
  }

  return (
    <div className="min-h-[calc(100vh-4rem)] flex items-center justify-center px-4 py-8 bg-gray-50">
      <div className="w-full max-w-md">
        <div className="rounded-2xl border border-gray-200 bg-white p-8 shadow-xl">
          <div className="text-center mb-8">
            <div className="text-4xl mb-3">🚀</div>
            <h1 className="text-2xl font-bold text-gray-900">Create Account</h1>
            <p className="text-sm text-gray-500 mt-1">Join Shopfinity today</p>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <Input id="firstName" label="First Name" placeholder="John"
                {...register('firstName')} error={errors.firstName?.message} />
              <Input id="lastName" label="Last Name" placeholder="Doe"
                {...register('lastName')} error={errors.lastName?.message} />
            </div>
            <Input id="email" label="Email" type="email" placeholder="you@example.com"
              {...register('email')} error={errors.email?.message} />
            <Input id="password" label="Password" type="password" placeholder="Min 8 chars, 1 uppercase, 1 number, 1 special"
              {...register('password')} error={errors.password?.message} />

            <Button type="submit" fullWidth isLoading={isSubmitting}>
              {isSubmitting ? 'Creating account…' : 'Create Account'}
            </Button>
          </form>

          <p className="text-center text-sm text-gray-500 mt-6">
            Already have an account?{' '}
            <Link href="/login" className="text-indigo-600 hover:text-indigo-500 font-medium">Sign in</Link>
          </p>
        </div>
      </div>
    </div>
  )
}
