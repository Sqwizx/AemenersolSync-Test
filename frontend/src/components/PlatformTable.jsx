// frontend/src/components/PlatformTable.jsx
export default function PlatformTable({ platforms }) {
  return (
    <table className="platform-table">
      <thead>
        <tr>
          <th>Platform</th>
          <th>Latitude</th>
          <th>Longitude</th>
          <th>Wells</th>
        </tr>
      </thead>
      <tbody>
        {platforms.map((platform) => (
          <tr key={platform.id}>
            <td>{platform.uniqueName ?? '—'}</td>
            <td>{platform.latitude ?? '—'}</td>
            <td>{platform.longitude ?? '—'}</td>
            <td>{platform.wells?.length ?? 0}</td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
