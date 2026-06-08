export default function AuthLayout({ children }) {
  return (
    <main className="app-shell">
      <header className="top-bar">
        <div className="logo-mark" aria-hidden="true" />
        <p className="logo-text">Function</p>
      </header>

      <section className="login-wrap">{children}</section>

      <footer className="footer-panel">
        <p className="copyright-text">
          © 2026 Task Manager. All rights reserved.
        </p>
      </footer>
    </main>
  )
}
