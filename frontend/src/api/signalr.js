// frontend/src/api/signalr.js
import * as signalR from '@microsoft/signalr'

const HUB_URL = 'https://localhost:7208/hubs/sync'

export function createSyncConnection({ onSyncCompleted, onUnauthorized }) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, { withCredentials: true })
    .withAutomaticReconnect()
    .build()

  connection.on('syncCompleted', (result) => onSyncCompleted(result))
  connection.on('unauthorized', () => onUnauthorized())

  connection.start().catch((err) => console.error('SignalR connection failed:', err))

  return connection
}
