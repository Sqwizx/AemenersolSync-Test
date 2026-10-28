// frontend/src/components/Dashboard.jsx
import PlatformTable from './PlatformTable'
import SkeletonTable from './SkeletonTable'

function formatLastSynced(lastSynced) {
  if (!lastSynced) return null
  return new Date(lastSynced).toLocaleString(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  })
}

export default function Dashboard({ platforms, lastSynced, onSync, onLogout, syncing, skeleton = false }) {
  const formatted = formatLastSynced(lastSynced)

  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="topbar-brand">
          <div className="topbar-mark">AE</div>
          <div>
            <h1>Aemenersol</h1>
            <p className="topbar-subtitle">Platform &amp; well sync</p>
          </div>
        </div>
        <div className="topbar-actions">
          <button className="btn btn-primary" onClick={onSync} disabled={syncing || skeleton}>
            {syncing ? 'Syncing…' : 'Sync Now'}
          </button>
          <button className="btn btn-ghost" onClick={onLogout} disabled={skeleton}>
            Logout
          </button>
        </div>
      </header>

      <main className="content">
        <div className="panel">
          <div className="panel-header">
            <div>
              <h2>Dashboard</h2>
              <p className="panel-meta">
                {platforms.length > 0
                  ? `${platforms.length} platform${platforms.length === 1 ? '' : 's'} synced`
                  : 'Platform and well records from Aemenersol'}
              </p>
            </div>
            <span className={`last-synced ${formatted ? '' : 'is-pending'}`}>
              <span className="last-synced-dot" />
              {formatted ? (
                <>
                  Last sync <strong>{formatted}</strong>
                </>
              ) : (
                'Not yet synced'
              )}
            </span>
          </div>

          {skeleton ? <SkeletonTable /> : <PlatformTable platforms={platforms} />}
        </div>
      </main>
    </div>
  )
}
