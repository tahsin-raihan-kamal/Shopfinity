
export interface LoginDto {
  email: string
  password: string
}

export interface RegisterDto {
  email: string
  password: string
  firstName: string
  lastName: string
}

export interface AuthUser {
  email: string
  roles: string[]
}

export interface AuthResponseDto {
  email: string
  roles: string[]
}

export interface ProductDto {
  id: string
  name: string
  slug: string
  description: string
  price: number
  stockQuantity: number
  categoryId: string
  imageUrl?: string
  createdAt: string
}

export interface CreateProductDto {
  name: string
  description: string
  price: number
  stockQuantity: number
  categoryId: string
  imageUrl?: string
}

export interface ProductSearchDto {
  searchTerm?: string

  search?: string
  categoryId?: string
  categorySlug?: string
  minPrice?: number
  maxPrice?: number
  pageNumber?: number
  pageSize?: number
}

export interface ProductSearchSuggestionDto {
  id: string
  name: string
  price: number
  imageUrl?: string
  slug: string
}

export interface PaginatedResult<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasNext: boolean
  hasPrev: boolean
}

export interface CategoryDto {
  id: string
  name: string
  slug: string
  description: string
  displayOrder?: number
}

export interface CreateCategoryDto {
  name: string
  description: string
}

export interface CartDto {
  id: string
  userId: string
  items: CartItemDto[]
  totalPrice: number
}

export interface CartItemDto {
  id: string
  productId: string
  productSlug?: string
  productName: string
  productImageUrl?: string
  unitPrice: number
  quantity: number
  subtotal: number
}

export interface AddToCartDto {
  productId: string
  quantity: number
}


export const ORDER_STATUS = {
  Pending: 'Pending',
  Processing: 'Processing',
  Shipped: 'Shipped',
  Delivered: 'Delivered',
  Cancelled: 'Cancelled',
} as const

export type OrderStatus = keyof typeof ORDER_STATUS

export const ORDER_STATUS_COLOR: Record<OrderStatus, string> = {
  Pending:    'bg-yellow-100 text-yellow-700 border-yellow-200',
  Processing: 'bg-blue-100 text-blue-700 border-blue-200',
  Shipped:    'bg-purple-100 text-purple-700 border-purple-200',
  Delivered:  'bg-green-100 text-green-700 border-green-200',
  Cancelled:  'bg-red-100 text-red-700 border-red-200',
}

export interface OrderItemDto {
  productId: string
  productName: string
  quantity: number
  unitPrice: number
}

export interface OrderResponseDto {
  id: string
  status: OrderStatus
  totalAmount: number
  createdAt: string
  items: OrderItemDto[]
}

export interface UpdateOrderStatusDto {
  status: OrderStatus
}

// ─── API Wrapper ──────────────────────────────────────────────────────────────
export interface ApiResponse<T> {
  success: boolean
  message: string
  data: T | null
  errors: string[] | null
}

// ─── Wishlist ─────────────────────────────────────────────────────────────────
export interface WishlistItemDto {
  id: string
  productId: string
  productSlug?: string
  productName: string
  productImageUrl?: string
  price: number
  createdAt: string
}

export interface AddWishlistItemDto {
  productId: string
}

// ─── Reviews ──────────────────────────────────────────────────────────────────
export interface ProductReviewDto {
  id: string
  userId: string
  userName: string
  productId: string
  rating: number
  title: string
  comment: string
  createdAt: string
}

export interface CreateReviewDto {
  productId: string
  rating: number
  title: string
  comment: string
}
