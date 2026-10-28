# DigitalWallet (.NET)

DigitalWallet is an .NET Core minimal API project based on [this learning path](https://docs.google.com/document/d/1UoL0WXWmgKmZ8LFrN7pCrpqhcgzL57QQ/edit#heading=h.oq60lkmrgu9k) backed by PostgreSQL. It creates wallets, funds them, transfers balances, and exposes wallet transaction and user activity history. It uses Kafka to move wallet events asynchronously into the history store.

This document describes the .NET application only. The React/Next.js frontend is a separate project under `Frontend/`.

## Technology and project layout

- **ASP.NET Core / .NET 10** hosts the HTTP API and background workers.
- **Entity Framework Core with Npgsql** persists wallet state, wallet transactions, history events, and outbox messages in PostgreSQL.
- **Confluent.Kafka** publishes and consumes Kafka messages.
- `Controllers/WalletController/` maps wallet and history routes.
- `Services/WalletService/` performs wallet operations and creates outbox records.
- `Services/EventService/` polls the PostgreSQL outbox and publishes pending records.
- `Services/KafkaService/` contains the Kafka producer and one consumer worker for each wallet event stream.
- `Services/HistoryService/` persists consumed events as transaction history.
- `Repository/` provides the EF Core data access and repository manager.
- `Data/` contains EF Core entities, DTOs, and `DatabaseContext`.
- `Migrations/` contains the PostgreSQL schema migrations.

## Run locally

### Prerequisites

- .NET 10 SDK
- PostgreSQL
- Kafka

The repository's `compose.yaml` can start local PostgreSQL and Kafka:

```sh
docker compose up -d postgres kafka
```

The supplied local configuration expects PostgreSQL at `localhost:5432` and Kafka at `localhost:9092`. When the API runs in a container on the Compose network, use the service hostnames instead (PostgreSQL `postgres:5432`, Kafka `kafka:29092`).

Set the PostgreSQL connection string and Kafka settings for the environment where the API runs. The configuration section below lists the expected keys.

Note: These are development keys.

Apply migrations explicitly; the API does not automatically migrate the database during startup:

```sh
dotnet ef database update
```

Then build and run the API:

```sh
dotnet build
dotnet run
```

## Configuration

Configuration is loaded from `appsettings.json`, then the optional environment-specific settings file. Standard .NET environment-variable configuration can override these values. For example:

```sh
ConnectionStrings__PostgresqlDatabase='Host=localhost;Port=5432;Database=<database>;Username=<user>;Password=<password>'
Kafka__WalletProducer__BootstrapServers=localhost:9092
Kafka__WalletTransferWorker__BootstrapServers=localhost:9092
```

The application binds these Kafka sections:

| Section | Purpose |
| --- | --- |
| `Kafka:WalletProducer` | Producer connection and delivery settings; configured topic names, fixed Kafka message key, and DLQ topic |
| `Kafka:WalletCreationWorker` | Consumer group and topic for `wallet-created` events |
| `Kafka:WalletFundingWorker` | Consumer group and topic for `wallet-funded` events |
| `Kafka:WalletTransferWorker` | Consumer group and topic for `wallet-transfer` events |

The producer configuration in the checked-in settings requests `Acks: All`, `EnableIdempotence: true`, three send retries, and a one-second retry backoff. Each consumer disables automatic offset commits and has its own consumer group. Keep producer and consumer topic names aligned.

The outbox poller sends to the topic recorded on each outbox row; each worker subscribes to the configured topic for its event stream.

## HTTP API

Wallet operations are exposed under `/api/wallets`:

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/wallets/` | List all wallets |
| `POST` | `/api/wallets/` | Create or return the wallet for a user |
| `GET` | `/api/wallets/{id}/` | Get a wallet |
| `POST` | `/api/wallets/{id}/fund` | Fund a wallet |
| `POST` | `/api/wallets/{id}/transfer` | Transfer between wallets |
| `GET` | `/api/wallets/{id}/history` | Get a wallet's stored transaction history |

User-scoped read routes are under `/api/users/{id}` and require a `Guid` route value:

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/users/{id}/wallets` | List wallets associated with a user |
| `GET` | `/api/users/{id}/activity` | Read persisted event history for a user |
| `GET` | `/api/users/{id}/transactions` | Read wallet transactions across that user's wallets |

Responses use the API's `ApiResponse<T>` envelope. Wallet write endpoints return after the PostgreSQL transaction completes (This includes the outbox write); they do not wait for the event to be published to Kafka or for the history consumer to persist it.

## Kafka event flow

Kafka is the message broker between wallet writes and the history event store. It is not the source of truth for wallet balances; PostgreSQL is.

### 1. The API writes wallet state and an outbox row

`WalletService` executes wallet creation, funding, and transfers using PostgreSQL transactions, which are atomic. Where an operation emits an event, it writes an `OutboxMessage` in the same database transaction as the wallet and wallet-transaction changes.

The outbox row contains a topic, a Kafka key, an event payload, a processed flag, and processing/error metadata. The payload carries fields such as `UserId`, `WalletId`, `TransactionId`, `Amount`, `EventType`, and `Timestamp`. A wallet creation event has no wallet transaction; funding and transfer events refer to the wallet transaction written by the operation.

The events are not published directly to Kafka so every business logic therefore commits or rolls back together.

### 2. `EventService` manages pending outbox records

[EventService](./Services/EventService/EventService.cs) is a [hosted background service](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-10.0&tabs=visual-studio).

It polls PostgreSQL for unprocessed rows, orders them by creation time, and selects up to 50 rows per batch. The implementation currently waits **60 seconds** between polls.

For each row, the service serializes the payload and populates an array to batch the call to [KafkaProducerService](./Services/KafkaService/KafkaProducerService/KafkaProducerService.cs) with the row's topic and key. If it receives a Kafka delivery result, it sets `Processed = true`, records `ProcessedAt`, clears `ErrorMessage`, and saves the changes to PostgreSQL. An exception thrown during per-row publishing is recorded in `ErrorMessage`; the row remains unprocessed so that it can be processed later.

If the producer returns `null`, the row is left unprocessed. In particular, `KafkaProducerService` catches `ProduceException` and throws , so that particular failure is copied into the outbox row's `ErrorMessage`.

### 3. Kafka consumers persist history

The application starts three hosted consumers:

| Worker | Topic role | Stored event |
| --- | --- | --- |
| `WalletCreationWorkerService` | Wallet creation | `WalletCreated` |
| `WalletFundWorkerService` | Wallet funding | `WalletFunded` |
| `WalletTransferWorkerService` | Transfers | `TransferCompleted` or `TransferFailed` |

Each worker deserializes the Kafka value as a `WalletEventDto`, maps it to a history DTO, and calls `HistoryService.SaveTransactionEvent`. On the normal success path, it commits the Kafka offset after saving the history event to PostgreSQL. History reads query PostgreSQL; they do not query Kafka directly.

A successful Kafka publish only means the event reached Kafka; it does not mean every history row is already visible through the API.

## What the Outbox solves

Publishing an event and committing a database update are two separate operations. Without an outbox, the API could update a wallet in PostgreSQL and then fail to publish the corresponding Kafka message, or publish an event for a database transaction that later rolls back.

The outbox avoids that: the wallet update and the intent to publish are persisted atomically in PostgreSQL. A separate event can publish the  intent later. This means Kafka can be temporarily unavailable without requiring the API to discard a successfully committed wallet operation, provided PostgreSQL remains available and the outbox row is persisted.

The outbox does not make PostgreSQL and Kafka one distributed transaction, guarantee exactly-once delivery, or ensure that a consumer has updated history before the API responds.

## Delivery behavior and guarantees

- Does it deal with eventual consistency?

Yes. Wallet state/outbox are committed first. Kafka publication and history persistence happen asynchronously, so `/activity` and `/history` can lag behind a successful wallet write. The outbox processing is intended to catch up while it can access PostgreSQL and Kafka.

- If Kafka is down but PostgreSQL works, can wallet operations continue?

Yes. The API write path commits the wallet change and outbox row in PostgreSQL without calling Kafka. The event remains pending and is retried by later polls. History will be delayed until publication and consumption recover.

- Does it retry failed operations?

 Partially The Kafka producer has three configured send retries. Unprocessed outbox rows are retried on future polling passes, with a batch limit of 50 and a 60-second poll interval. This is an event-publication retry, not an automatic retry of an HTTP wallet operation. Consumer failures are not generally retried: they are sent to the dead letter queue path and the offset is then committed. See the limitations below.

- Does it correlate events across services?

 Events carry user, wallet, transaction, type, and timestamp fields, and the configured Kafka key is shared as `wallet-service`. These fields help identify an event and partition the stream, additionaly, there is a dedicated cross-service `CorrelationId`. A transfer writes separate sender and receiver wallet transactions with separate transaction IDs, so the two events share one explicit transfer ID.

### Consumer and relay limitations to account for

The consumer worker saves the `TransactionEvent` and updates a matching inbox receipt inside one PostgreSQL transaction. When an event has a correlation ID, it marks matching inbox rows that are not already received, records `ReceivedAt`, and requires exactly one row to have been updated before saving the history event. This gives the consumer an atomic persistence boundary for a single matching inbox row.

The principal persisted records are:

- **Wallet**: user ID, balance, and timestamps.
- **WalletTransaction**: wallet ID, amount, status, type, and timestamps. Types are `Fund`, `TransferOut`, and `TransferIn`; statuses are `Completed` and `Failed`.
- **OutboxMessage**: topic, key, payload, processed status, processing time, and last recorded error.
- **TransactionEvent**: event and payload fields stored by the Kafka consumer and returned by activity/history endpoints.

Event types are `WalletCreated`, `WalletFunded`, `TransferCompleted`, and `TransferFailed`. Successful transfers write a sender-side and receiver-side wallet transaction and enqueue corresponding transfer events. An insufficient-balance attempt writes a failed sender transaction and a `TransferFailed` outbox event, while the API returns a bad-request result.

## Operational notes

- Keep PostgreSQL available to both API request processing and the outbox relay.
- Keep Kafka available to the relay, all three consumers, and the DLQ producer path.
- Monitor pending outbox count/age, `ErrorMessage`, Kafka consumer lag, and the DLQ. The current implementation logs with `Console.WriteLine`; it does not expose dedicated health checks or metrics for these queues.
- Ensure the configured DLQ topic exists and is reachable.
- Review topic retention, replication, and broker availability settings for the deployment. The checked-in Compose Kafka service is a single-node local-development setup, not a production durability configuration.
- Treat the outbox and Kafka as operationally important persisted data. Do not delete unprocessed outbox records as a way to recover from a broker outage.
