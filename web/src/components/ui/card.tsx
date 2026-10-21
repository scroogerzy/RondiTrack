import type { ComponentProps } from 'react'
import { cn } from '@/lib/utils'

/** Semantic card primitive used by the StokvelCard view. */
export function Card({ className, ...props }: ComponentProps<'article'>) {
  return <article data-slot="card" className={cn('card', className)} {...props} />
}

export function CardHeader({ className, ...props }: ComponentProps<'div'>) {
  return <div data-slot="card-header" className={cn('card-header', className)} {...props} />
}

/** A real h3 heading, not a styled div. */
export function CardTitle({ className, ...props }: ComponentProps<'h3'>) {
  return <h3 data-slot="card-title" className={cn('card-title', className)} {...props} />
}

export function CardContent({ className, ...props }: ComponentProps<'div'>) {
  return <div data-slot="card-content" className={cn('card-content', className)} {...props} />
}
