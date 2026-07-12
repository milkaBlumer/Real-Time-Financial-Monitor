# SQLINK Frontend Assignment

React + TypeScript frontend with two routes:

- `/add`: transaction simulator that posts mock transactions to backend ingestion API.
- `/monitor`: live dashboard that listens to backend SignalR hub and renders transactions in real time.

## Features

- Clear route-level separation of concerns.
- Transaction generator with manual inputs and forced status.
- Real-time monitor with status colors and error-only filter.
- Connection state badge (`connecting/open/closed/error`).
- UI performance protection for burst traffic (message batching via `requestAnimationFrame`).

## Backend Configuration

Create a `.env` file (or set environment variables):

```bash
VITE_API_BASE_URL=https://localhost:7276
VITE_WS_BASE_URL=wss://localhost:7276
```

Expected backend endpoints:

- HTTP ingestion: `POST {VITE_API_BASE_URL}/api/Ingest`
- SignalR hub: `{VITE_API_BASE_URL}/hub/transactions`
- SignalR event name: `transaction`

## Run

```bash
npm install
npm run dev
```

## Build

```bash
npm run build
```
