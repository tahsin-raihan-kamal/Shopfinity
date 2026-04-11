import { NextRequest, NextResponse } from 'next/server'
import { cookies } from 'next/headers'

const API_URL = process.env.NEXT_PUBLIC_API_URL!
const isProd = process.env.NODE_ENV === 'production'

export async function POST(request: NextRequest) {
  try {
    const body = await request.json()

    const backendRes = await fetch(`${API_URL}/api/v1/Auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    })

    const json = await backendRes.json()

    if (!backendRes.ok || !json.success) {
      return NextResponse.json(
        { success: false, message: json.message ?? 'Login failed' },
        { status: backendRes.status }
      )
    }

    const { token, refreshToken, email, roles } = json.data
    const cookieStore = await cookies()

    // ── Access token (HttpOnly, short-lived) ──────────────────────────────
    cookieStore.set('shopfinity_token', token, {
      httpOnly: true,
      secure: isProd,
      sameSite: 'lax',
      path: '/',
      maxAge: 60 * 60, // 1 hour
    })

    // ── Refresh token (HttpOnly, long-lived, path-scoped) ─────────────────
    cookieStore.set('shopfinity_refresh', refreshToken, {
      httpOnly: true,
      secure: isProd,
      sameSite: 'lax',
      path: '/api/auth/refresh',
      maxAge: 60 * 60 * 24 * 7, // 7 days
    })

    // Must match backend CsrfValidationMiddleware + axios (cookie name XSRF-TOKEN).
    const xsrf = crypto.randomUUID().replace(/-/g, '')
    cookieStore.set('XSRF-TOKEN', xsrf, {
      httpOnly: false,
      secure: isProd,
      sameSite: 'lax',
      path: '/',
      maxAge: 60 * 60 * 24 * 7,
    })

    return NextResponse.json({ success: true, data: { email, roles } })
  } catch {
    return NextResponse.json(
      { success: false, message: 'Internal server error' },
      { status: 500 }
    )
  }
}
