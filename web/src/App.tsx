import { ContributionCard } from './components/ContributionCard'
import { StokvelCard } from './components/StokvelCard'
import { sampleContributions, sampleStokvels } from './data/sample'

export default function App() {
  return (
    <main className="app-shell">
      <header className="site-header">
        <a className="brand" href="/" aria-label="RondiTrack home">
          <span className="brand-mark">R</span>
          <span>RondiTrack</span>
        </a>
        <span className="header-caption">STOKVEL MANAGEMENT</span>
      </header>

      <section className="welcome">
        <p className="eyebrow">YOUR FINANCIAL CIRCLE</p>
        <h1>Stay on top of your stokvels.</h1>
        <p className="welcome-copy">
          Track contributions, follow your cycle, and keep every member on the same page.
        </p>
      </section>

      <section aria-labelledby="stokvel-heading">
        <div className="section-heading">
          <div>
            <p className="eyebrow">OVERVIEW</p>
            <h2 id="stokvel-heading">Your stokvels</h2>
          </div>
          <span className="section-count">{sampleStokvels.length} stokvels</span>
        </div>
        <div className="stokvel-grid">
          {sampleStokvels.map((stokvel) => (
            <StokvelCard key={stokvel.id} {...stokvel} />
          ))}
        </div>
      </section>

      <section aria-labelledby="contributions-heading">
        <div className="section-heading">
          <div>
            <p className="eyebrow">RECENT ACTIVITY</p>
            <h2 id="contributions-heading">Contributions</h2>
          </div>
        </div>
        <div className="contribution-list">
          {sampleContributions.map((contribution) => (
            <ContributionCard key={contribution.id} {...contribution} />
          ))}
        </div>
      </section>

      <footer className="site-footer">
        <span>RondiTrack · Assignment 7.1</span>
        <span>Transparent stokvel management.</span>
      </footer>
    </main>
  )
}
