import { InputHTMLAttributes, forwardRef } from 'react'

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string
  error?: string
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, className = '', id, ...props }, ref) => (
    <div className="flex flex-col gap-1.5">
      {label && (
        <label htmlFor={id} className="text-sm font-medium text-gray-700">
          {label}
        </label>
      )}
      <input
        ref={ref}
        id={id}
        {...props}
        className={`
          w-full rounded-xl bg-white border px-4 py-2.5 text-gray-900 placeholder-gray-400
          focus:outline-none focus:ring-2 focus:ring-indigo-500/50 transition-all
          ${error ? 'border-red-500 focus:ring-red-500/40' : 'border-gray-300 focus:border-indigo-500'}
          ${className}
        `}
      />
      {error && <p className="text-xs text-red-500">{error}</p>}
    </div>
  )
)

Input.displayName = 'Input'
