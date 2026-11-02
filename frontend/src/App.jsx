// frontend/src/App.jsx
import { useEffect, useState, useCallback, useRef } from 'react'
import Dashboard from './components/Dashboard'
import LoginModal from './components/LoginModal'
import { createSyncConnection } from './api/signalr'
import * as api from './api/client'

export default function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false)
  const [platforms, setPlatforms] = useState([])
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)
  const [totalCount, setTotalCount] = useState(0)
  const [lastSynced, setLastSynced] = useState(null)
  const [syncing, setSyncing] = useState(false)
  const [checkingStatus, setCheckingStatus] = useState(true)

  const pagingRef = useRef({ page, pageSize })
  pagingRef.current = { page, pageSize }

  const refreshData = useCallback(async (targetPage) => {
    try {
      const { page: currentPage, pageSize: currentPageSize } = pagingRef.current
      const data = await api.getSyncData(targetPage ?? currentPage, currentPageSize)
      setPlatforms(data?.items ?? [])
      setPage(data?.page ?? 1)
      setPageSize(data?.pageSize ?? currentPageSize)
      setTotalCount(data?.totalCount ?? 0)
    } catch (err) {
      if (err instanceof api.UnauthorizedError) {
        setIsAuthenticated(false)
      }
    }
  }, [])

  useEffect(() => {
    api.getStatus()
      .then((status) => {
        setIsAuthenticated(status.authenticated)
        if (status.authenticated) refreshData(1)
      })
      .finally(() => setCheckingStatus(false))
  }, [refreshData])

  useEffect(() => {
    const connection = createSyncConnection({
      onSyncCompleted: (result) => {
        setLastSynced(result.timestamp)
        refreshData()
      },
      onUnauthorized: () => {
        setIsAuthenticated(false)
      },
    })
    return () => connection.stop()
  }, [refreshData])

  async function handleLogin() {
    await api.login()
    setIsAuthenticated(true)
    await refreshData()
  }

  async function handleLogout() {
    await api.logout()
    setIsAuthenticated(false)
    setPlatforms([])
  }

  async function handleSync() {
    setSyncing(true)
    try {
      const result = await api.triggerSync()
      setLastSynced(result.timestamp)
      await refreshData()
    } catch (err) {
      if (err instanceof api.UnauthorizedError) {
        setIsAuthenticated(false)
      }
    } finally {
      setSyncing(false)
    }
  }

  async function handlePageChange(nextPage) {
    await refreshData(nextPage)
  }

  if (checkingStatus) return null

  if (isAuthenticated) {
    return (
      <Dashboard
        platforms={platforms}
        lastSynced={lastSynced}
        onSync={handleSync}
        onLogout={handleLogout}
        syncing={syncing}
        page={page}
        pageSize={pageSize}
        totalCount={totalCount}
        onPageChange={handlePageChange}
      />
    )
  }

  return (
    <>
      <div className="blurred-backdrop">
        <Dashboard
          platforms={[]}
          lastSynced={null}
          onSync={() => {}}
          onLogout={() => {}}
          syncing={false}
          skeleton
        />
      </div>
      <LoginModal onLogin={handleLogin} />
    </>
  )
}
