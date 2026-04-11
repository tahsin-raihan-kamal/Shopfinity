import ProductDetailClient from './ProductDetailClient'
import type { Metadata, ResolvingMetadata } from 'next'
import { serverFetch } from '@/lib/serverFetch'

export async function generateMetadata(
  props: { params: Promise<{ slug: string }> },
  parent: ResolvingMetadata
): Promise<Metadata> {
  const params = await props.params;
  
  try {
    const product = await serverFetch<any>(`api/v1/Products/${params.slug}`, { next: { revalidate: 60 } })
    
    if (!product) return { title: 'Product Not Found | Shopfinity' }
    
    return {
      title: `${product.name} | Shopfinity`,
      description: product.description,
      openGraph: {
        images: product.imageUrl ? [product.imageUrl] : [],
      }
    }
  } catch (error) {
    return { title: 'Product Details | Shopfinity' }
  }
}

export default function Page({ params }: { params: Promise<{ slug: string }> }) {
  return <ProductDetailClient params={params} />
}
