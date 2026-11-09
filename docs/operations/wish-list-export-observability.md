# Wish list export observability and rollout

Applies to `exportList`, `exportListPaged` and `GET /_v/wishlist/export-lists`.
Exports are serialized per process; a second concurrent export is rejected
fail-fast (REST HTTP 429 with `Retry-After: 5`, GraphQL error code
`EXPORT_IN_PROGRESS`).

## Rollout plan

1. **Canary cluster:** one `iostore` cluster before global rollout.
2. **Baseline window:** 24 hours before rollout (OOM count, restarts, export volume).
3. **Canary observation:** at least 24 hours.
4. **Decision window:** seven complete days after rollout.

## Rollback triggers

- Any increase in OOM rate against the same weekday baseline.
- Sustained `EXPORT_IN_PROGRESS` above 5% of export attempts for 15 minutes.
- p95 duration above the 60-second service timeout.
- New `invalid_response` or `no_progress` results above 1% of attempts.

## Metrics (Prometheus)

Labels are limited to `operation` (`full` | `paged`) and `result`
(`success`, `busy`, `cancelled`, `masterdata_error`, `invalid_response`, `no_progress`).
Families: `wishlist_export_operations_total`, `wishlist_export_records_read`,
`wishlist_export_records_returned`, `wishlist_export_bytes_read`,
`wishlist_export_pages_read`, `wishlist_export_duration_seconds`.

Operations by outcome:

```promql
sum by (operation, result) (
  increase(wishlist_export_operations_total[24h])
)
```

p95 records read:

```promql
histogram_quantile(
  0.95,
  sum by (le, operation) (
    rate(wishlist_export_records_read_bucket{result="success"}[7d])
  )
)
```

p99 bytes read:

```promql
histogram_quantile(
  0.99,
  sum by (le, operation) (
    rate(wishlist_export_bytes_read_bucket{result="success"}[7d])
  )
)
```

OOM events per cluster:

```promql
sum(increase(container_oom_events_total{
  namespace="vendor-vtex",
  pod=~"vtex-wish-list.*"
}[24h])) by (cluster)
```

## Logs (account attribution)

```logsql
_stream:{vtex.search_index="io_3p_logs", app=~"vtex.wish-list@.*"}
  workflow_type:"WishListExportCompleted"
| stats
    count() operations,
    max(recordsRead) max_records,
    max(bytesRead) max_bytes,
    max(durationMs) max_duration_ms
  by (account, operation)
```

Logs may be sampled and are used for account attribution only; the Prometheus
histograms are the source of global quantiles. Logs never contain emails,
shopper IDs, list content, payloads or scroll tokens.

## Seven-day decision gate

After seven full days record:

- p50, p95, p99 and max for records, bytes, pages and duration;
- full vs. paged operation volume;
- busy rejection rate;
- OOM and restart counts by cluster;
- top accounts from sampled logs;
- recommendation: keep compatible, introduce a measured limit, deprecate the
  full export, or design an asynchronous export.

The phase-2 contract change is out of scope for this implementation.
