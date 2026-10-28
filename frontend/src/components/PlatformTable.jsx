// frontend/src/components/PlatformTable.jsx
import { useState, Fragment } from 'react'

function WellsPanel({ wells }) {
  if (!wells || wells.length === 0) {
    return <div className="wells-empty">No wells recorded for this platform.</div>
  }

  return (
    <table className="well-table">
      <thead>
        <tr>
          <th>Well</th>
          <th>Latitude</th>
          <th>Longitude</th>
          <th>Updated</th>
        </tr>
      </thead>
      <tbody>
        {wells.map((well) => (
          <tr key={well.id}>
            <td className="col-name">{well.uniqueName ?? '—'}</td>
            <td className="col-num">{well.latitude ?? '—'}</td>
            <td className="col-num">{well.longitude ?? '—'}</td>
            <td className="col-num">
              {well.updatedAt ? new Date(well.updatedAt).toLocaleDateString() : '—'}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}

export default function PlatformTable({ platforms }) {
  const [expanded, setExpanded] = useState(() => new Set())

  if (!platforms || platforms.length === 0) {
    return <div className="empty-state">No platforms synced yet. Click "Sync Now" to pull data.</div>
  }

  function toggle(id) {
    setExpanded((prev) => {
      const next = new Set(prev)
      if (next.has(id)) {
        next.delete(id)
      } else {
        next.add(id)
      }
      return next
    })
  }

  return (
    <table className="platform-table">
      <thead>
        <tr>
          <th aria-hidden="true"></th>
          <th>Platform</th>
          <th>Latitude</th>
          <th>Longitude</th>
          <th>Wells</th>
        </tr>
      </thead>
      <tbody>
        {platforms.map((platform) => {
          const wellCount = platform.wells?.length ?? 0
          const isOpen = expanded.has(platform.id)
          return (
            <Fragment key={platform.id}>
              <tr
                className={`row-platform ${isOpen ? 'is-open' : ''} ${wellCount > 0 ? 'is-expandable' : ''}`}
                onClick={() => wellCount > 0 && toggle(platform.id)}
              >
                <td className="col-chevron">
                  {wellCount > 0 && <span className={`chevron ${isOpen ? 'is-open' : ''}`}>›</span>}
                </td>
                <td className="col-name">{platform.uniqueName ?? '—'}</td>
                <td className="col-num">{platform.latitude ?? '—'}</td>
                <td className="col-num">{platform.longitude ?? '—'}</td>
                <td>
                  <span className={`well-badge ${wellCount === 0 ? 'is-zero' : ''}`}>{wellCount}</span>
                </td>
              </tr>
              {isOpen && (
                <tr className="row-wells">
                  <td colSpan={5}>
                    <WellsPanel wells={platform.wells} />
                  </td>
                </tr>
              )}
            </Fragment>
          )
        })}
      </tbody>
    </table>
  )
}
