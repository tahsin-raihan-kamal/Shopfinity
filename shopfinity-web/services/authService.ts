import { LoginInput, RegisterInput } from '@/lib/schemas'
import { AuthResponseDto } from '@/types'

export const authService = {
  async login(data: LoginInput): Promise<AuthResponseDto> {
    const res = await fetch('/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
      credentials: 'include',
    })
    const json = await res.json()
    if (!json.success) throw new Error(json.message ?? 'Login failed')
    return json.data
  },

  async register(data: RegisterInput): Promise<AuthResponseDto> {
    const res = await fetch('/api/auth/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
      credentials: 'include',
    })
    const json = await res.json()
    if (!json.success) throw new Error(json.message ?? 'Registration failed')
    return json.data
  },

  async logout(): Promise<void> {
    await fetch('/api/auth/logout', { method: 'POST', credentials: 'include' })
  },
}
