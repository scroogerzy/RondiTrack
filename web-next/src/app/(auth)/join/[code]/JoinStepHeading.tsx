'use client';
import { useEffect, useRef } from 'react';
export default function JoinStepHeading({ children, step }: { children: React.ReactNode; step: number }) {
  const heading = useRef<HTMLHeadingElement>(null);
  useEffect(() => { heading.current?.focus(); }, [step]);
  return <h1 ref={heading} tabIndex={-1} className="text-3xl font-bold">{children}</h1>;
}
