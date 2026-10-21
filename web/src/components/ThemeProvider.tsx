import { useEffect, useState, type ReactNode } from 'react'
import { ThemeContext, type Theme } from './theme-context'

function readInitialTheme(): Theme {
  try {
    const stored = localStorage.getItem('ronditrack-theme')
    return stored === 'light' || stored === 'dark' || stored === 'system' ? stored : 'system'
  } catch {
    return 'system'
  }
}

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setTheme] = useState<Theme>(readInitialTheme)

  useEffect(() => {
    // Keep the document theme in sync; the CSS media query keeps System live.
    document.documentElement.dataset.theme = theme
    try {
      localStorage.setItem('ronditrack-theme', theme)
    } catch {
      // Storage is optional; private browsing must not prevent the view from rendering.
    }
  }, [theme])

  return <ThemeContext.Provider value={{ theme, setTheme }}>{children}</ThemeContext.Provider>
}
