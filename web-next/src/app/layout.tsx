import type { Metadata } from 'next';
import './globals.css';
import AppProviders from '@/components/AppProviders';

export const metadata: Metadata = {
  title: 'RondiTrack',
  description: 'Track stokvel contributions and cycles'
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return <html lang="en"><body><AppProviders>{children}</AppProviders></body></html>;
}
