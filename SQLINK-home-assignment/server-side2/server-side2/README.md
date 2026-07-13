# Backend - Cloud-Ready Distributed Architecture

This service is an ASP.NET Core 8 backend for real-time transaction monitoring.

## System Context

The full project has:

- Frontend (React + SignalR client)
  - `POST /api/Ingest` to submit transactions
  - subscribes to `/hub/transactions` for live updates
- Backend (this service)
  - Ingestion API (`IngestController`)
  - Realtime hub (`TransactionHub`)
  - Persistence (EF Core + SQLite)
  - Cache and cross-pod event bus integration (Redis)

## Runtime Architecture

High-level flow:

1. Client sends `POST /api/Ingest`.
2. Backend validates and normalizes the transaction.
3. Transaction is persisted to SQLite (source of truth in current implementation).
4. Transaction is cached in Redis for fast `GET /api/Ingest` reads.
5. Backend publishes the event to Redis Pub/Sub channel `transactions_channel`.
6. All backend pods subscribed to the channel receive the event.
7. Every pod broadcasts the event to its local SignalR clients.

This guarantees realtime fan-out across replicas.

## Bonus Requirement: Distributed Architecture

### The Challenge

With multiple backend replicas (for example, 5 pods), SignalR connections are local to each pod.
Without a shared bus:

- clients on Pod A only get events produced on Pod A
- events produced on Pod B/C/D/E are missed by Pod A clients

### Implemented Solution

Redis Pub/Sub is used as the synchronization backbone:

- Publisher: any pod that ingests a transaction publishes to Redis
- Subscribers: every pod listens to the same channel
- Local delivery: subscriber forwards each message to SignalR clients on that pod

Implemented components:

- `Program.cs`
  - Redis connection registration
  - `IRealtimePublisher -> RedisRealtimePublisher`
  - hosted background subscriber registration
- `Infrastructure/Messaging/RedisMessageBroker.cs`
  - message publish to `transactions_channel`
- `Infrastructure/Messaging/RedisSubscriptionService.cs`
  - channel subscription and forwarding to SignalR
- `Infrastructure/Realtime/SignalRPublisher.cs`
  - `Clients.All.SendAsync("transaction", tx)`

## Cloud Readiness (DevOps)

### Production Dockerfile

`../Dockerfile` is production-oriented:

- multi-stage build
- Alpine SDK/runtime base images
- release publish output only in runtime image
- non-root execution user
- app exposed on port `8080`

### Kubernetes Manifests

Kubernetes files exist in `../k8s/`:

- `deployment.yaml`
  - `replicas: 5`
  - readiness/liveness probes on `/health`
  - CPU/memory requests and limits
  - configuration via ConfigMap + Secret
- `service.yaml`
  - internal `ClusterIP` service
- `configmap.yaml`
  - environment/runtime configuration
- `secret.yaml`
  - secret values such as `Redis__ConnectionString`
- `hpa.yaml`
  - autoscaling policy (CPU target)

## Important Accuracy Notes

To reflect the current code exactly:

- Realtime synchronization is distributed and cloud-ready.
- Persistence is currently SQLite file-based (`transactions.db`) inside each pod.

This means realtime events are synchronized across pods, but database state is not globally shared between replicas unless a shared external database is introduced.