// frontend/src/components/SkeletonTable.jsx
export default function SkeletonTable() {
  const rows = Array.from({ length: 5 })
  return (
    <table className="platform-table skeleton">
      <thead>
        <tr>
          <th>Platform</th>
          <th>Latitude</th>
          <th>Longitude</th>
          <th>Wells</th>
        </tr>
      </thead>
      <tbody>
        {rows.map((_, i) => (
          <tr key={i}>
            <td><div className="skeleton-bar" /></td>
            <td><div className="skeleton-bar" /></td>
            <td><div className="skeleton-bar" /></td>
            <td><div className="skeleton-bar" /></td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
