function formatDate(value) {
  return value ? new Date(value).toLocaleDateString() : '—'
}

export default function PlatformTable({ platforms, selectedPlatformId, onSelect, loading = false }) {
  function handleKeyDown(event, platformId) {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault()
      onSelect?.(platformId)
    }
  }

  return (
    <div className="platform-table-region" aria-busy={loading}>
      {!platforms || platforms.length === 0 ? (
        <div className="empty-state">No platforms synced yet. Click "Sync Now" to pull data.</div>
      ) : (
        <table className="platform-table">
          <thead>
            <tr>
              <th>Platform</th>
              <th>Latitude</th>
              <th>Longitude</th>
              <th>Created</th>
              <th>Wells</th>
            </tr>
          </thead>
          <tbody>
            {platforms.map((platform) => {
              const wellCount = platform.wells?.length ?? 0
              const isSelected = selectedPlatformId === platform.id
              return (
                <tr
                  key={platform.id}
                  className={`row-platform is-selectable ${isSelected ? 'is-selected' : ''}`}
                  onClick={() => onSelect?.(platform.id)}
                  onKeyDown={(event) => handleKeyDown(event, platform.id)}
                  tabIndex={0}
                >
                  <td className="col-name">{platform.uniqueName ?? '—'}</td>
                  <td className="col-num">{platform.latitude ?? '—'}</td>
                  <td className="col-num">{platform.longitude ?? '—'}</td>
                  <td className="col-num">{formatDate(platform.createdAt)}</td>
                  <td>
                    <span className={`well-badge ${wellCount === 0 ? 'is-zero' : ''}`}>{wellCount}</span>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      )}

      {loading && (
        <div className="table-loading-overlay" role="status">
          <span className="sync-spinner" aria-hidden="true" />
          <span>Syncing platform data…</span>
        </div>
      )}
    </div>
  )
}
