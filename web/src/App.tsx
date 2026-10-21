import { useState } from 'react'
import { Moon, Sun, Monitor, Landmark, ArrowUpRight } from 'lucide-react'
import { Button } from './components/ui/button'
import { useTheme } from './components/useTheme'
import { ContributionList } from './components/ContributionList'
import { StokvelCard } from './components/StokvelCard'
import { sampleContributions, sampleCycles, sampleStokvels } from './data/sample'

export default function App() {
  const { theme, setTheme } = useTheme()
  const [soonestFirst, setSoonestFirst] = useState(false)
  const sortedStokvels = [...sampleStokvels].sort((a,b) => {
    const av = a.dueDate ?? '9999-12-31'; const bv = b.dueDate ?? '9999-12-31'; return soonestFirst ? av.localeCompare(bv) : bv.localeCompare(av)
  })
  return <div className="app-root" data-theme={theme}>
    <main className="app-shell mx-auto w-full max-w-6xl px-4 sm:px-6 lg:px-8">
      <header className="site-header"><a className="brand" href="/" aria-label="RondiTrack home"><span className="brand-mark"><Landmark size={20} /></span><span>RondiTrack</span></a><div className="header-actions"><span className="header-caption">STOKVEL MANAGEMENT</span><label className="sr-only" htmlFor="theme-select">Colour theme</label><select id="theme-select" className="theme-select" value={theme} onChange={(event) => setTheme(event.target.value as 'light' | 'dark' | 'system')}><option value="system">System</option><option value="light">Light</option><option value="dark">Dark</option></select><span aria-hidden="true">{theme === 'dark' ? <Moon size={16}/> : theme === 'light' ? <Sun size={16}/> : <Monitor size={16}/>}</span></div></header>
      <section className="welcome"><p className="eyebrow">YOUR FINANCIAL CIRCLE</p><h1>Clear contributions.<br/><span>Stronger circles.</span></h1><p className="welcome-copy">A quick, meeting-ready view of your stokvels, cycle progress and member contributions.</p></section>
      <section aria-labelledby="stokvel-heading"><div className="section-heading"><div><p className="eyebrow">OVERVIEW</p><h2 id="stokvel-heading">Your stokvels</h2></div><div className="section-actions"><span className="section-count">{sampleStokvels.length} stokvels</span><Button variant="outline" className="button button-outline" onClick={() => setSoonestFirst((value) => !value)}>Due date {soonestFirst ? '↑' : '↓'} <ArrowUpRight size={14}/></Button></div></div><div className="stokvel-grid">{sortedStokvels.map((stokvel) => <StokvelCard key={stokvel.id} {...stokvel}/>)}</div></section>
      <section aria-labelledby="contributions-heading"><div className="section-heading"><div><p className="eyebrow">CYCLE-BY-CYCLE</p><h2 id="contributions-heading">Contribution history</h2></div><span className="section-count">{sampleContributions.length} records</span></div><ContributionList cycles={sampleCycles} contributions={sampleContributions}/></section>
      <footer className="site-footer"><span>RondiTrack · Assignment 7.2</span><span>Fixture data for UI development · API integration still required</span></footer>
    </main>
  </div>
}
