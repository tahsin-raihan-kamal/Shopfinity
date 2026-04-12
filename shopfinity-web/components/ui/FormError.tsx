interface FormErrorProps { message?: string }
export function FormError({ message }: FormErrorProps) {
  if (!message) return null
  return <p className="text-xs text-red-500 mt-1 flex items-center gap-1">
    <span>⚠</span>{message}
  </p>
}
