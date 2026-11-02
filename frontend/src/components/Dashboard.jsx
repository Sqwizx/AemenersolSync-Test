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

export default function Dashboard({
  platforms,
  lastSynced,
  onSync,
  onLogout,
  syncing,
  skeleton = false,
  page = 1,
  pageSize = 20,
  totalCount = 0,
  onPageChange,
}) {
  const formatted = formatLastSynced(lastSynced)
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize))

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
            <div className="panel-sync">
              <button className="btn btn-primary" onClick={onSync} disabled={syncing || skeleton}>
                {syncing ? 'Syncing…' : 'Sync Now'}
              </button>
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
          </div>

          {skeleton ? <SkeletonTable /> : <PlatformTable platforms={platforms} />}

          {!skeleton && totalCount > 0 && (
            <div className="pager">
              <button
                className="btn btn-ghost"
                onClick={() => onPageChange?.(page - 1)}
                disabled={page <= 1}
              >
                Prev
              </button>
              <span className="pager-status">
                Page {page} of {totalPages}
              </span>
              <button
                className="btn btn-ghost"
                onClick={() => onPageChange?.(page + 1)}
                disabled={page >= totalPages}
              >
                Next
              </button>
            </div>
          )}
        </div>
      </main>
    </div>
  )
}
