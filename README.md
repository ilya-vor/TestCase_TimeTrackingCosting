# Учёт трудозатрат и стоимость работ по проектам

Учёт табеля рабочего времени, стоимость по ставкам на дату записи, закрытые периоды,
отчёт по проектам за месяц.

## Запуск

Требуется Docker с Docker Compose. Один запуск поднимает MongoDB (replica set),
backend API и фронтенд, и сам наполняет базу тестовыми данными из задания.

1. `git clone <repo> && cd TestCase_TimeTrackingCosting`
2. `docker compose up --build`

### Запуск без Docker (для разработки)

```bash
# 1. MongoDB как однонодовый replica set (нужен для транзакций)
docker compose up -d mongo mongo-init

# 2. Backend (порт 5080)
cd backend
dotnet run --project src/TimeTracking.Api --urls http://localhost:5080

# 3. Frontend (порт 5173, проксирует /api на 5080)
cd frontend
npm install && npm run dev
```

Для подключения к MongoDB с хоста нужен `directConnection=true`:
`ConnectionStrings__Mongo="mongodb://localhost:27017/?replicaSet=rs0&directConnection=true"`.

## Тестовые данные

База заполняется автоматически при старте API, если она пуста.
Чтобы пересоздать данные с нуля выполните:

```bash
docker compose exec mongo mongosh --quiet --eval "db.getSiblingDB('time_tracking').dropDatabase()"
docker compose restart api
```

Данные из раздела «Приёмочные проверки»: Иванов (500/600 ₽/ч),
Петрова (700 ₽/ч), проекты П-001 (20 000 ₽) и П-002 (5 000 ₽), четыре записи
табеля за февраль–март 2026.

## API

| Метод | Путь | Назначение |
|---|---|---|
| GET | `/api/time-entries` | постраничный список за месяц, фильтры `year, month, employeeId, projectId, page, pageSize`; в ответе — ФИО, шифр проекта, часы, применённая ставка, стоимость, признак переработки, версия |
| PUT | `/api/time-entries` | создать запись |
| POST | `/api/time-entries/{id}` | изменить запись (нужна `expectedVersion`) |
| DELETE | `/api/time-entries/{id}` | удалить запись |
| GET | `/api/reports/projects` | отчёт по проектам за месяц (`year, month`), агрегация на стороне MongoDB |
| GET | `/api/employees`, `/api/projects` | справочники |
| PUT | `/api/employees/{id}/rates` | сменить ставку сотрудника (`from, value`) — нужно для приёмочного сценария 8 |
| POST | `/api/periods/close`, `/api/periods/open` | закрыть/открыть месяц |

Ошибки бизнес-правил: `400`/`409` с телом `{ "code": "...", "message": "..." }` на русском.

## Тесты

```bash
cd backend && dotnet test
```

40 юнит-тестов на бизнес-правила: выбор ставки по дате, лимит 24 ч/день,
переработка 12 ч, закрытый период, границы периода проекта, кратность часов 0,5,
округление денег.

## Ссылки

- `NOTES.md` — допущения, решения, обоснование индексов, что не доделано.
- `REVIEW.md` — ответ по части 1 (code review).
