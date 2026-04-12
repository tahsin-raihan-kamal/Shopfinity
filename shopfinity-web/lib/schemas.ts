import { z } from 'zod'

// ─── Auth Schemas ─────────────────────────────────────────────────────────────
export const loginSchema = z.object({
  email:    z.string().email('Please enter a valid email'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
})

export const registerSchema = z.object({
  firstName: z.string().min(1, 'First name is required').max(100),
  lastName:  z.string().min(1, 'Last name is required').max(100),
  email:     z.string().email('Please enter a valid email'),
  password:  z.string()
    .min(8, 'Password must be at least 8 characters')
    .regex(/[A-Z]/, 'Must include an uppercase letter')
    .regex(/[a-z]/, 'Must include a lowercase letter')
    .regex(/[0-9]/, 'Must include a number')
    .regex(/[^a-zA-Z0-9]/, 'Must include a special character (e.g. @#!$)'),
})

// ─── Product Schema ───────────────────────────────────────────────────────────
export const productSchema = z.object({
  name:          z.string().min(1, 'Product name is required').max(200),
  description:   z.string().max(2000).optional(),
  price:         z.number().min(0.01, 'Price must be at least BDT 0.01').max(999_999.99),
  stockQuantity: z.number().int('Must be a whole number').min(0, 'Cannot be negative'),
  categoryId:    z.string().uuid('Please select a valid category'),
  slug:          z.string().regex(/^[a-z0-9-]+$/, 'Only lowercase letters, numbers, hyphens').optional(),
  imageUrl:      z.string().optional(),
})

// ─── Category Schema ──────────────────────────────────────────────────────────
export const categorySchema = z.object({
  name:        z.string().min(1, 'Category name is required').max(100),
  description: z.string().max(500).optional(),
})

// ─── Checkout Schema ──────────────────────────────────────────────────────────
export const checkoutSchema = z.object({
  address: z.string().min(5, 'Address must be at least 5 characters'),
  city:    z.string().min(1, 'City is required'),
  zip:     z.string().min(3, 'ZIP code is required'),
})

// ─── Cart Schema ──────────────────────────────────────────────────────────────
export const addToCartSchema = z.object({
  quantity: z.coerce.number().int().min(1, 'Minimum 1').max(99, 'Maximum 99'),
})

// ─── Inferred Types ───────────────────────────────────────────────────────────
export type LoginInput     = z.infer<typeof loginSchema>
export type RegisterInput  = z.infer<typeof registerSchema>
export type ProductInput   = z.infer<typeof productSchema>
export type CategoryInput  = z.infer<typeof categorySchema>
export type CheckoutInput  = z.infer<typeof checkoutSchema>
export type AddToCartInput = z.infer<typeof addToCartSchema>
