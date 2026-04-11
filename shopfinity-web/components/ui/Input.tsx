import { InputHTMLAttributes, forwardRef } from 'react'

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string
  error?: string
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, className = '', id, ...props }, ref) => (
    <div className="flex flex-col gap-1.5">
      {label && (
        <label htmlFor={id} className="text-sm font-medium text-gray-200">
          {label}
        </label>
      )}
      <input
        ref={ref}
        id={id}
        {...props}
        className={`
          w-full rounded-xl bg-white/5 border px-4 py-2.5 text-white placeholder-gray-500
          focus:outline-none focus:ring-2 focus:ring-indigo-500/50 transition-all
          ${error ? 'border-red-500/60 focus:ring-red-500/40' : 'border-white/10 focus:border-indigo-500/60'}
          ${className}
        `}
      />
      {error && <p className="text-xs text-red-400">{error}</p>}
    </div>
  )
)

Input.displayName = 'Input'
