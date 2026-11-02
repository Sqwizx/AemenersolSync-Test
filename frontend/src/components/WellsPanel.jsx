function formatDate(value) {
  return value ? new Date(value).toLocaleDateString() : '—'
}

export default function WellsPanel({ platform }) {
  const wells = platform?.wells ?? []
  const platformName = platform?.uniqueName ?? 'Selected platform'

  return (
    <aside className="wells-pane" aria-live="polite">
      <div className="wells-pane-header">
        <div>
          <h3>{platform ? platformName : 'No platform selected'}</h3>
        </div>
        {platform && (
          <span
            className={`well-badge ${wells.length === 0 ? 'is-zero' : ''}`}
            aria-label={`${wells.length} related well${wells.length === 1 ? '' : 's'}`}
          >
            {wells.length}
          </span>
        )}
      </div>

      {!platform ? (
        <div className="wells-empty wells-empty-selection">
          Select a platform to view its related wells.
        </div>
      ) : wells.length === 0 ? (
        <div className="wells-empty">No wells recorded for this platform.</div>
      ) : (
        <div className="well-list">
          {wells.map((well) => (
            <article className="well-card" key={well.id}>
              <h4>{well.uniqueName ?? '—'}</h4>
              <dl className="well-details">
                <div>
                  <dt>Latitude</dt>
                  <dd>{well.latitude ?? '—'}</dd>
                </div>
                <div>
                  <dt>Longitude</dt>
                  <dd>{well.longitude ?? '—'}</dd>
                </div>
                <div>
                  <dt>Created</dt>
                  <dd>{formatDate(well.createdAt)}</dd>
                </div>
                <div>
                  <dt>Updated</dt>
                  <dd>{formatDate(well.updatedAt)}</dd>
                </div>
              </dl>
            </article>
          ))}
        </div>
      )}
    </aside>
  )
}
