import { NextRequest, NextResponse } from 'next/server'
import { cookies } from 'next/headers'

const API_URL = process.env.NEXT_PUBLIC_API_URL!
const isProd = process.env.NODE_ENV === 'production'

export async function POST(request: NextRequest) {
  try {
    const cookieStore = await cookies()
    const refreshToken = cookieStore.get('shopfinity_refresh')?.value

    if (!refreshToken) {
      return NextResponse.json({ success: false, message: 'No refresh token' }, { status: 401 })
    }

    const backendRes = await fetch(`${API_URL}/api/v1/Auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    })

    const json = await backendRes.json()

    if (!backendRes.ok || !json.success) {
      // Clear all auth cookies
      cookieStore.delete('shopfinity_token')
      cookieStore.delete('shopfinity_refresh')
      cookieStore.delete('XSRF-TOKEN')
      return NextResponse.json({ success: false, message: 'Session expired' }, { status: 401 })
    }

    const { token, refreshToken: newRefreshToken } = json.data

    cookieStore.set('shopfinity_token', token, {
      httpOnly: true, secure: isProd, sameSite: 'lax', path: '/', maxAge: 60 * 60,
    })
    cookieStore.set('shopfinity_refresh', newRefreshToken, {
      httpOnly: true, secure: isProd, sameSite: 'lax', path: '/api/auth/refresh', maxAge: 60 * 60 * 24 * 7,
    })
    const xsrf = crypto.randomUUID().replace(/-/g, '')
    cookieStore.set('XSRF-TOKEN', xsrf, {
      httpOnly: false, secure: isProd, sameSite: 'lax', path: '/', maxAge: 60 * 60 * 24 * 7,
    })

    return NextResponse.json({ success: true })
  } catch {
    return NextResponse.json({ success: false, message: 'Refresh failed' }, { status: 500 })
  }
}
