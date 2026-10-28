'use client';

export default function AppError({ reset }: { error: Error & { digest?: string }; reset: () => void }) {
  return (
    <section role="alert" className="space-y-3 rounded border border-red-300 p-5">
      <h2 className="text-xl font-semibold">Something went wrong</h2>
      <p>Please try again. If the problem continues, contact support.</p>
      <button className="rounded border px-4 py-2" onClick={() => reset()}>Try again</button>
    </section>
  );
}
