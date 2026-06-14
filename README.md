# NICK2.0-core

## Local PostgreSQL

Copy `.env.example` to `.env`, then start PostgreSQL and the API:

```bash
make local-run
```

The app applies pending EF Core migrations on startup.

Useful commands:

```bash
make db-up
make db-down
make db-reset
make test
```

## Railway

The service includes a `Dockerfile` for Railway deployments. Railway should provide `PORT`; the container binds to it automatically.

Set these variables in Railway:

```text
PostgreSql__Host
PostgreSql__Port
PostgreSql__Database
PostgreSql__Username
PostgreSql__Password
SmartMoving__BaseUrl
SmartMoving__ApiKey
SmartMoving__WebhookToken
```

SmartMoving webhook custom header:

```text
X-SmartMoving-Webhook-Token: same-value-as-SmartMoving__WebhookToken
```

`SmartMoving_WebhookToken` is also supported as a fallback variable name.
