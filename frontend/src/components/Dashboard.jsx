// frontend/src/components/Dashboard.jsx
import { useState } from 'react'
import PlatformTable from './PlatformTable'
import SkeletonTable from './SkeletonTable'
import WellsPanel from './WellsPanel'

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
  const [selectedPlatformId, setSelectedPlatformId] = useState(null)
  const formatted = formatLastSynced(lastSynced)
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize))
  const selectedPlatform = platforms.find((platform) => platform.id === selectedPlatformId)

  function handlePlatformSelect(platformId) {
    setSelectedPlatformId((currentId) => (currentId === platformId ? null : platformId))
  }

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
                  ? `Showing ${platforms.length} of ${totalCount} platform${totalCount === 1 ? '' : 's'}`
                  : 'Platform and well records from Aemenersol'}
              </p>
            </div>
            <div className="panel-sync">
              <button className="btn btn-primary btn-sync" onClick={onSync} disabled={syncing || skeleton} aria-busy={syncing}>
                {syncing && <span className="sync-spinner" aria-hidden="true" />}
                <span>{syncing ? 'Syncing…' : 'Sync Now'}</span>
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

          <div className="dashboard-split">
            <div className="platform-pane">
              {skeleton ? (
                <SkeletonTable />
              ) : (
                <PlatformTable
                  platforms={platforms}
                  selectedPlatformId={selectedPlatformId}
                  onSelect={handlePlatformSelect}
                  loading={syncing}
                />
              )}
            </div>
            <WellsPanel platform={skeleton ? null : selectedPlatform} />
          </div>

          {!skeleton && totalCount > 0 && (
            <div className="pager">
              <button
                className="btn btn-ghost"
                onClick={() => onPageChange?.(page - 1)}
                disabled={page <= 1 || syncing}
              >
                Prev
              </button>
              <span className="pager-status">
                Page {page} of {totalPages}
              </span>
              <button
                className="btn btn-ghost"
                onClick={() => onPageChange?.(page + 1)}
                disabled={page >= totalPages || syncing}
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
