import axios from 'axios'
import Cookies from 'js-cookie'
import toast from 'react-hot-toast'

const axiosInstance = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5049',
  timeout: 10_000,
  withCredentials: true,
})
axiosInstance.interceptors.request.use(config => {
  const csrf = Cookies.get('XSRF-TOKEN') // Standardized name
  if (csrf && ['post', 'put', 'delete', 'patch'].includes(config.method?.toLowerCase() ?? '')) {
    config.headers['X-XSRF-TOKEN'] = csrf
  }
  return config
})

let isRefreshing = false
let failedQueue: Array<{ resolve: (v: unknown) => void; reject: (e: unknown) => void }> = []

const processQueue = (error: unknown) => {
  failedQueue.forEach(p => (error ? p.reject(error) : p.resolve(null)))
  failedQueue = []
}

axiosInstance.interceptors.response.use(
  res => {
    // Auto-unwrap ApiResponse<T> so service functions get T directly
    if (res.data && typeof res.data === 'object' && 'success' in res.data) {
      if (res.data.success === true) {
        return { ...res, data: res.data.data }
      }
      return Promise.reject(new Error(res.data.message ?? 'Request failed'))
    }
    return res
  },
  async error => {
    const originalRequest = error.config

    // ── Network / timeout errors ───────────────────────────────────────────
    if (!error.response) {
      const isTimeout = error.code === 'ECONNABORTED'
      toast.error(
        isTimeout
          ? 'Request timed out. Please try again.'
          : 'Network error. Please check your connection.'
      )
      return Promise.reject(error)
    }

    // ── 401: try to refresh token silently ────────────────────────────────
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        }).then(() => axiosInstance(originalRequest))
          .catch(e => Promise.reject(e))
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        await fetch('/api/auth/refresh', { method: 'POST', credentials: 'include' })
        processQueue(null)
        return axiosInstance(originalRequest)
      } catch (refreshError) {
        processQueue(refreshError)
        // Redirect to login
        if (typeof window !== 'undefined') window.location.href = '/login'
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    // ── Other HTTP errors: show toast ─────────────────────────────────────
    if (error.response?.status !== 401) {
      const msg = error.response?.data?.message ?? 'Something went wrong.'
      toast.error(msg)
    }

    return Promise.reject(error)
  }
)

export default axiosInstance
