## 1.0.1 — 2026-06-12

- **Fix:** host bridge loop 0.5 с только на headless, не на dedicated клиенте

## 1.0.0 — 2026-06-12

- Выделен из `CoopInventoryModsFikaFix` v1.0.14
- Headless host bridge для PocketRoulette `PocketPacket` (30014)
- Патч `FikaServer.RegisterPacketsAndTypes` + lifetime loop регистрации
- Клиент: нативный поток PocketRoulette `SendPocketItem` (без клиентских патчей)
