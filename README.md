# ContractorIQ

AI-powered job intelligence platform for UK IT contractors. Aggregates contract roles, provides IR35 screening, CV tailoring, market rate benchmarking, and application tracking.

Built by [Yug Solutions Ltd](https://yugsolutions.co.uk).

## Stack

| Layer | Technology |
|-------|-----------|
| API | .NET 10, ASP.NET Core, Entity Framework Core |
| Frontend | React 19, Vite 6, TypeScript |
| Database | PostgreSQL 16 |
| Scraper | Python 3.12, Scrapy |
| Auth | JWT |
| Billing | Stripe |
| AI | OpenAI (CV tailoring, IR35 analysis, job matching) |
| Email | Resend |
| Containerisation | Docker Compose |

## Repository Structure

```
 ContractorIQ/
├── ContractorIQ.API/     # .NET 10 REST API
├── src/
│   ├── contractoriq-web/  # React/Vite/TypeScript frontend
│   └── contractoriq-scraper/ # Python/Scrapy job scraper
├── docker/postgres/       # DB init scripts
└── compose.yaml           # Full local stack
```

## Local Development

### 1. Start the database

`ash
docker compose up -d postgres
```

### 2. Configure secrets

`ash
cd ContractorIQ.API
dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
dotnet user-secrets set "OpenAI:ApiKey" "sk-proj-..."
dotnet user-secrets set "Resend:ApiKey" "re_..."
```

### 3. Run the API

`ash
cd ContractorIQ.API
dotnet run
```

### 4. Run the frontend

`ash
cd src/contractoriq-web
npm install
npm run dev
```

## Licence

Private — © 2026 Yug Solutions Ltd. All rights reserved.
