import Link from 'next/link'

export function Footer() {
  return (
    <footer className="bg-gray-900 text-white pt-16 pb-8">
      <div className="max-w-7xl mx-auto px-6 grid grid-cols-1 md:grid-cols-4 gap-8 mb-12">
        <div>
          <h3 className="text-xl font-bold mb-4">Shopfinity</h3>
          <p className="text-gray-400 text-sm">Your ultimate destination for premium tech gear, from gaming rigs to professional audio.</p>
        </div>
        <div>
          <h4 className="font-semibold mb-4">Shop</h4>
          <ul className="space-y-2 text-sm text-gray-400">
            <li><Link href="/" className="hover:text-white transition-colors">Laptops</Link></li>
            <li><Link href="/" className="hover:text-white transition-colors">Audio</Link></li>
            <li><Link href="/" className="hover:text-white transition-colors">Mobile</Link></li>
            <li><Link href="/" className="hover:text-white transition-colors">Gaming</Link></li>
          </ul>
        </div>
        <div>
          <h4 className="font-semibold mb-4">Support</h4>
          <ul className="space-y-2 text-sm text-gray-400">
            <li><Link href="/" className="hover:text-white transition-colors">Contact Us</Link></li>
            <li><Link href="/" className="hover:text-white transition-colors">Returns</Link></li>
            <li><Link href="/" className="hover:text-white transition-colors">Shipping Info</Link></li>
            <li><Link href="/" className="hover:text-white transition-colors">Track Order</Link></li>
          </ul>
        </div>
        <div>
          <h4 className="font-semibold mb-4">Stay Connected</h4>
          <p className="text-sm text-gray-400 mb-4">Subscribe to get special offers and gear updates.</p>
          <div className="flex">
            <input type="email" placeholder="Your email" className="bg-gray-800 text-white px-4 py-2 rounded-l-md w-full focus:outline-none" />
            <button className="bg-indigo-600 px-4 py-2 rounded-r-md font-semibold hover:bg-indigo-700 transition-colors">Sign Up</button>
          </div>
        </div>
      </div>
      <div className="max-w-7xl mx-auto px-6 border-t border-gray-800 pt-8 flex flex-col md:flex-row justify-between items-center text-xs text-gray-500">
        <p>&copy; {new Date().getFullYear()} Shopfinity Inc. All rights reserved.</p>
        <div className="flex gap-4 mt-4 md:mt-0">
          <Link href="/" className="hover:text-gray-300">Privacy Policy</Link>
          <Link href="/" className="hover:text-gray-300">Terms of Service</Link>
        </div>
      </div>
    </footer>
  )
}
