// frontend/src/components/Dashboard.jsx
import PlatformTable from './PlatformTable'

export default function Dashboard({ platforms, lastSynced, onSync, onLogout, syncing }) {
  return (
    <div className="dashboard">
      <header>
        <h1>Platform / Well Sync Dashboard</h1>
        <div className="controls">
          <button onClick={onSync} disabled={syncing}>
            {syncing ? 'Syncing...' : 'Sync Now'}
          </button>
          <button onClick={onLogout}>Logout</button>
        </div>
        {lastSynced && <p className="last-synced">Last synced: {new Date(lastSynced).toLocaleString()}</p>}
      </header>
      <PlatformTable platforms={platforms} />
    </div>
  )
}
