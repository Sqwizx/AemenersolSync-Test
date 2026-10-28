// frontend/src/components/LoginModal.jsx
import { useState } from 'react'

export default function LoginModal({ onLogin }) {
  const [error, setError] = useState(null)
  const [loading, setLoading] = useState(false)

  async function handleSubmit(e) {
    e.preventDefault()
    setLoading(true)
    setError(null)
    try {
      await onLogin()
    } catch (err) {
      setError('Login failed. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="modal-overlay">
      <div className="modal">
        <div className="modal-mark">AE</div>
        <h2>Login to Dashboard</h2>
        <p className="modal-subtitle">Sign in to view and sync Aemenersol platform &amp; well data.</p>
        <form onSubmit={handleSubmit}>
          <button type="submit" className="btn btn-primary btn-block" disabled={loading}>
            {loading ? 'Logging in…' : 'Log in'}
          </button>
        </form>
        {error && <p className="modal-error">{error}</p>}
      </div>
    </div>
  )
}
