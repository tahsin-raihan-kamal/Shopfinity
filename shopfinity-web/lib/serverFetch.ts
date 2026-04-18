import { cookies } from 'next/headers'

interface FetchOptions extends RequestInit {
  params?: Record<string, string>
}

export class ApiError extends Error {
  public statusCode: number
  public errors: string[]

  constructor(message: string, statusCode: number, errors: string[] = []) {
    super(message)
    this.name = 'ApiError'
    this.statusCode = statusCode
    this.errors = errors
  }
}

export async function serverFetch<T>(endpoint: string, options: FetchOptions = {}): Promise<T> {
  const baseUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5049'
  let url = `${baseUrl}${endpoint.startsWith('/') ? '' : '/'}${endpoint}`

  if (options.params) {
    const searchParams = new URLSearchParams(options.params)
    url += `?${searchParams.toString()}`
  }

  const cookieStore = await cookies()
  const headers = new Headers(options.headers)

 
  const allCookies = cookieStore.getAll()
  const cookieString = allCookies.map(c => `${c.name}=${c.value}`).join('; ')
  if (cookieString) {
    headers.set('Cookie', cookieString)
  }

  const method = options.method || 'GET'
  if (['POST', 'PUT', 'PATCH', 'DELETE'].includes(method.toUpperCase())) {
    const csrfToken = cookieStore.get('XSRF-TOKEN')?.value
    if (csrfToken) {
      headers.set('X-XSRF-TOKEN', csrfToken)
    }
  }

  if (!headers.has('Content-Type') && !(options.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json')
  }

  try {
    const response = await fetch(url, {
      ...options,
      headers,
      signal: AbortSignal.timeout(15000), // 15s hard timeout
    })

    // 4. Handle Standard ApiResponse/ApiErrorResponse Formats
    const data = await response.json().catch(() => null)

    if (!response.ok) {
      const message = data?.message || 'An unexpected error occurred'
      const statusCode = data?.statusCode || response.status
      const errors = data?.errors || []
      
      throw new ApiError(message, statusCode, errors)
    }

    // Unwrap ApiResponse<T> returning just T
    if (data && typeof data === 'object' && 'success' in data) {
      return data.data as T
    }

    return data as T
  } catch (err: any) {
    if (err.name === 'TimeoutError' || err.name === 'AbortError') {
      throw new ApiError('The request timed out. Please check your connection or try again later.', 408)
    }
    throw err
  }
}
