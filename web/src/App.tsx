import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { ArrowUpRight, Landmark, LogIn, LogOut, Monitor, Moon, Sun } from 'lucide-react'
import { useQueryClient } from '@tanstack/react-query'
import { Button } from './components/ui/button'
import { useTheme } from './components/useTheme'
import { StokvelCard } from './components/StokvelCard'
import { ErrorView } from './components/ErrorView'
import { useHome, useCycles, useContributions } from './api/queries'
import { login, logout, onSignedOut } from './auth/session'
import { formatCents } from './lib/money'
import './App.css'

export default function App() {
  const { theme, setTheme } = useTheme()
  const queryClient = useQueryClient()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [loginError, setLoginError] = useState('')
  const [loggingIn, setLoggingIn] = useState(false)
  const [selectedStokvelId, setSelectedStokvelId] = useState('')
  const [soonestFirst, setSoonestFirst] = useState(false)
  const [signedOut, setSignedOut] = useState(false)

  useEffect(() => onSignedOut(() => {
    queryClient.clear()
    setSelectedStokvelId('')
    setSignedOut(true)
  }), [queryClient])

  const home = useHome()
  const stokvels = useMemo(() => home.data?.stokvels ?? [], [home.data?.stokvels])

  const effectiveStokvelId = stokvels.some(
    (stokvel) => stokvel.stokvelId === selectedStokvelId,
  )
    ? selectedStokvelId
    : (stokvels[0]?.stokvelId ?? '')

  const selectedStokvel = stokvels.find(
    (stokvel) => stokvel.stokvelId === effectiveStokvelId,
  )

  const cyclesQuery = useCycles(effectiveStokvelId)
  const cycles = useMemo(
    () => cyclesQuery.data?.pages.flatMap((page) => page.items) ?? [],
    [cyclesQuery.data],
  )
  const activeCycle = cycles[0]
  const contributionsQuery = useContributions(
    effectiveStokvelId,
    activeCycle?.id ?? '',
  )
  const contributions = useMemo(
    () => contributionsQuery.data?.pages.flatMap((page) => page.items) ?? [],
    [contributionsQuery.data],
  )

  const sortedStokvels = useMemo(() => [...stokvels].sort((a, b) => {
    const av = a.nextPayoutDateUtc ?? '9999-12-31'
    const bv = b.nextPayoutDateUtc ?? '9999-12-31'
    return soonestFirst ? av.localeCompare(bv) : bv.localeCompare(av)
  }), [stokvels, soonestFirst])

  async function handleLogin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setLoggingIn(true)
    setLoginError('')

    try {
      await login(email, password)
      setSignedOut(false)
      await queryClient.invalidateQueries()
    } catch {
      setLoginError('Email or password is incorrect.')
    } finally {
      setLoggingIn(false)
    }
  }

  async function handleLogout() {
    await logout()
    queryClient.clear()
    setSelectedStokvelId('')
    setSignedOut(true)
  }

  if (signedOut || (home.error && !home.data)) {
    return (
      <div className="app-root" data-theme={theme}>
        <main className="app-shell mx-auto w-full max-w-6xl px-4 sm:px-6 lg:px-8">
          <header className="site-header">
            <a className="brand" href="/" aria-label="RondiTrack home">
              <span className="brand-mark"><Landmark size={20} /></span>
              <span>RondiTrack</span>
            </a>
            <div className="header-actions">
              <label className="sr-only" htmlFor="theme-select">Colour theme</label>
              <select id="theme-select" className="theme-select" value={theme}
                onChange={(event) => setTheme(event.target.value as 'light' | 'dark' | 'system')}>
                <option value="system">System</option>
                <option value="light">Light</option>
                <option value="dark">Dark</option>
              </select>
              <span aria-hidden="true">{theme === 'dark' ? <Moon size={16}/> : theme === 'light' ? <Sun size={16}/> : <Monitor size={16}/>}</span>
            </div>
          </header>

          <section className="welcome">
            <p className="eyebrow">YOUR FINANCIAL CIRCLE</p>
            <h1>Welcome back.<br/><span>Let's get you in.</span></h1>
            <p className="welcome-copy">Sign in to view your stokvels and contribution history.</p>
          </section>

          <form className="mx-auto grid max-w-md gap-4 rounded-2xl border p-6"
            onSubmit={handleLogin}>
            <label className="grid gap-1">
              <span>Email address</span>
              <input autoComplete="username" type="email" required value={email}
                onChange={(event) => setEmail(event.target.value)} />
            </label>
            <label className="grid gap-1">
              <span>Password</span>
              <input autoComplete="current-password" type="password" required value={password}
                onChange={(event) => setPassword(event.target.value)} />
            </label>
            {loginError && <p role="alert">{loginError}</p>}
            {home.error && !signedOut && (
              <ErrorView error={home.error} onRetry={() => void home.refetch()} />
            )}
            <Button type="submit" disabled={loggingIn}>
              <LogIn size={16} /> {loggingIn ? 'Signing in…' : 'Sign in'}
            </Button>
          </form>
        </main>
      </div>
    )
  }

  return (
    <div className="app-root" data-theme={theme}>
      <main className="app-shell mx-auto w-full max-w-6xl px-4 sm:px-6 lg:px-8">
        <header className="site-header">
          <a className="brand" href="/" aria-label="RondiTrack home">
            <span className="brand-mark"><Landmark size={20} /></span>
            <span>RondiTrack</span>
          </a>
          <div className="header-actions">
            <span className="header-caption">STOKVEL MANAGEMENT</span>
            <label className="sr-only" htmlFor="theme-select">Colour theme</label>
            <select id="theme-select" className="theme-select" value={theme}
              onChange={(event) => setTheme(event.target.value as 'light' | 'dark' | 'system')}>
              <option value="system">System</option>
              <option value="light">Light</option>
              <option value="dark">Dark</option>
            </select>
            <span aria-hidden="true">{theme === 'dark' ? <Moon size={16}/> : theme === 'light' ? <Sun size={16}/> : <Monitor size={16}/>}</span>
            <Button variant="outline" className="button button-outline" onClick={() => void handleLogout()}>
              <LogOut size={14} /> Sign out
            </Button>
          </div>
        </header>

        <section className="welcome">
          <p className="eyebrow">YOUR FINANCIAL CIRCLE</p>
          <h1>Clear contributions.<br/><span>Stronger circles.</span></h1>
          <p className="welcome-copy">A quick, meeting-ready view of your stokvels, cycle progress and member contributions.</p>
        </section>

        {home.isLoading && (
          <section aria-label="Loading stokvels" className="grid gap-4">
            <div className="h-8 animate-pulse rounded-lg bg-neutral-200" />
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="h-40 animate-pulse rounded-2xl bg-neutral-200" />
              <div className="h-40 animate-pulse rounded-2xl bg-neutral-200" />
            </div>
          </section>
        )}

        {home.error && <ErrorView error={home.error} onRetry={() => void home.refetch()} />}

        {!home.isLoading && !home.error && (
          <>
            <section aria-labelledby="stokvel-heading">
              <div className="section-heading">
                <div>
                  <p className="eyebrow">OVERVIEW</p>
                  <h2 id="stokvel-heading">Your stokvels</h2>
                </div>
                <div className="section-actions">
                  <span className="section-count">{stokvels.length} stokvels</span>
                  <Button variant="outline" className="button button-outline"
                    onClick={() => setSoonestFirst((value) => !value)}>
                    Due date {soonestFirst ? '↑' : '↓'} <ArrowUpRight size={14}/>
                  </Button>
                </div>
              </div>

              {stokvels.length === 0 ? (
                <p className="rounded-xl border p-6">You haven't joined a stokvel yet.</p>
              ) : (
                <div className="stokvel-grid">
                  {sortedStokvels.map((stokvel) => (
                    <div key={stokvel.stokvelId}
                      className={selectedStokvelId === stokvel.stokvelId ? 'rounded-2xl ring-2 ring-offset-2' : 'rounded-2xl'}>
                      <button
                        type="button"
                        className="w-full text-left"
                        aria-pressed={selectedStokvelId === stokvel.stokvelId}
                        onClick={() => setSelectedStokvelId(stokvel.stokvelId)}>
                        <StokvelCard
                          id={stokvel.stokvelId}
                          name={stokvel.name}
                          role={stokvel.role === 'Treasurer' ? 'Treasurer' : 'Member'}
                          cycleNumber={stokvel.currentCycle}
                          cycleName={null}
                          cycleStatus={(() => {
                            const current = cycles.find(
                              (cycle) => cycle.periodNumber === stokvel.currentCycle
                            )
                            return current && ['Open', 'Closed', 'PaidOut'].includes(current.status)
                              ? current.status as 'Open' | 'Closed' | 'PaidOut'
                              : null
                          })()}
                          dueDate={cycles.find(
                            (cycle) => cycle.periodNumber === stokvel.currentCycle
                          )?.endDate ?? null}
                          paidThisCycleCents={stokvel.amountPaidThisCycleCents}
                          stillOwedCents={stokvel.amountOwedCents}
                          nextPayoutAt={stokvel.nextPayoutDateUtc}
                          arrearsCents={stokvel.arrearsCents}
                        />
                      </button>
                    </div>
                  ))}
                </div>
              )}
            </section>

            <section aria-labelledby="contributions-heading">
              <div className="section-heading">
                <div>
                  <p className="eyebrow">CYCLE-BY-CYCLE</p>
                  <h2 id="contributions-heading">Contribution history</h2>
                </div>
                <label className="grid gap-1 text-sm">
                  <span>Selected stokvel</span>
                  <select value={effectiveStokvelId}
                    onChange={(event) => setSelectedStokvelId(event.target.value)}>
                    {stokvels.map((stokvel) => (
                      <option key={stokvel.stokvelId} value={stokvel.stokvelId}>
                        {stokvel.name}
                      </option>
                    ))}
                  </select>
                </label>
              </div>

              {selectedStokvel && (
                <p className="mb-4 text-sm">
                  {selectedStokvel.name} · Paid this cycle: {formatCents(selectedStokvel.amountPaidThisCycleCents)} · Owed: {formatCents(selectedStokvel.amountOwedCents)}
                </p>
              )}

              {cyclesQuery.isLoading && <p className="rounded-xl border p-5">Loading contribution cycles…</p>}
              {cyclesQuery.error && <ErrorView error={cyclesQuery.error} onRetry={() => void cyclesQuery.refetch()} />}
              {!cyclesQuery.isLoading && !cyclesQuery.error && cycles.length === 0 && (
                <p className="rounded-xl border p-5">No contribution cycles are available for this stokvel yet.</p>
              )}

              {cycles.length > 0 && (
                <>
                  <div className="mb-4 flex flex-wrap gap-2">
                    {cycles.map((cycle) => (
                      <span key={cycle.id} className="rounded-full border px-3 py-1 text-sm">
                        Cycle {cycle.periodNumber} · {formatCents(cycle.totalCollectedCents)} collected
                      </span>
                    ))}
                  </div>

                  {contributionsQuery.isLoading && <p className="rounded-xl border p-5">Loading contributions…</p>}
                  {contributionsQuery.error && <ErrorView error={contributionsQuery.error} onRetry={() => void contributionsQuery.refetch()} />}
                  {!contributionsQuery.isLoading && !contributionsQuery.error && contributions.length === 0 && (
                    <p className="rounded-xl border p-5">No contributions recorded for the current cycle yet.</p>
                  )}
                  {contributions.length > 0 && (
                    <div className="space-y-3" aria-label="Live contributions">
                      {contributions.map((contribution) => (
                        <article key={contribution.id} className="contribution-card">
                          <div className="contribution-main">
                            <strong>
                              {formatCents(Math.round(Number(contribution.amount) * 100))}
                            </strong>
                            <p>
                              Cycle {contribution.cycle} · Recorded{' '}
                              {new Intl.DateTimeFormat('en-ZA', {
                                dateStyle: 'medium',
                                timeStyle: 'short',
                                timeZone: 'Africa/Johannesburg',
                              }).format(new Date(contribution.recordedAt))}
                            </p>
                            <p className="text-sm">
                              Contributor reference: {contribution.userId}
                            </p>
                          </div>
                        </article>
                      ))}
                    </div>
                  )}
                  {cyclesQuery.hasNextPage && (
                    <Button variant="outline" className="button button-outline mt-4"
                      disabled={cyclesQuery.isFetchingNextPage}
                      onClick={() => void cyclesQuery.fetchNextPage()}>
                      {cyclesQuery.isFetchingNextPage ? 'Loading…' : 'Load more cycles'}
                    </Button>
                  )}
                  {contributionsQuery.hasNextPage && (
                    <Button variant="outline" className="button button-outline mt-4"
                      disabled={contributionsQuery.isFetchingNextPage}
                      onClick={() => void contributionsQuery.fetchNextPage()}>
                      {contributionsQuery.isFetchingNextPage ? 'Loading…' : 'Load more contributions'}
                    </Button>
                  )}
                </>
              )}
            </section>
          </>
        )}

        <footer className="site-footer">
          <span>RondiTrack · Assignment 7.3</span>
          <span>Live data from the RondiTrack API</span>
        </footer>
      </main>
    </div>
  )
}
