import { NextResponse } from 'next/server'
import { cookies } from 'next/headers'

const API_URL = process.env.NEXT_PUBLIC_API_URL!

export async function POST() {
  const cookieStore = await cookies()
  const refreshToken = cookieStore.get('shopfinity_refresh')?.value

  if (refreshToken) {
    try {
      await fetch(`${API_URL}/api/v1/Auth/logout`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken }),
      })
    } catch {
      // Ignore errors if backend is unreachable during logout
    }
  }

  cookieStore.delete('shopfinity_token')
  cookieStore.delete('shopfinity_refresh')
  cookieStore.delete('XSRF-TOKEN')
  
  return NextResponse.json({ success: true, message: 'Logged out.' })
}
