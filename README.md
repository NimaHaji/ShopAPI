<p align="center">
  <img src="https://img.shields.io/badge/C%23-13-2B2B2B?logo=csharp&logoColor=239120" alt="C#">
  <img src="https://img.shields.io/badge/.NET-9.0-2B2B2B?logo=dotnet&logoColor=512BD4" alt=".NET">
  <img src="https://img.shields.io/badge/EF%20Core-9.0-2B2B2B?logo=dotnet&logoColor=512BD4" alt="Entity Framework Core">
  <img src="https://img.shields.io/badge/FluentValidation-2B2B2B?logoColor=F6A800" alt="FluentValidation">
  <img src="https://img.shields.io/badge/Serilog-2B2B2B?logo=serilog&logoColor=45A1D7" alt="Serilog">
  <img src="https://img.shields.io/badge/Swagger-2B2B2B?logo=swagger&logoColor=85EA2D" alt="Swagger">
</p>

<p align="center">
  <img src="https://img.shields.io/badge/SQL%20Server-2B2B2B?logo=microsoftsqlserver&logoColor=CC2927" alt="SQL Server">
  <img src="https://img.shields.io/badge/Redis-2B2B2B?logo=redis&logoColor=DC382D" alt="Redis">
  <img src="https://img.shields.io/badge/Docker-2B2B2B?logo=docker&logoColor=2496ED" alt="Docker">
</p>

<p align="center">
  <img src="https://img.shields.io/badge/OpenTelemetry-2B2B2B?logo=opentelemetry&logoColor=F5A800" alt="OpenTelemetry">
  <img src="https://img.shields.io/badge/Prometheus-2B2B2B?logo=prometheus&logoColor=E6522C" alt="Prometheus">
  <img src="https://img.shields.io/badge/Grafana-2B2B2B?logo=grafana&logoColor=F46800" alt="Grafana">
  <img src="https://img.shields.io/badge/Loki-2B2B2B?logo=grafana&logoColor=F2CC0C" alt="Loki">
  <img src="https://img.shields.io/badge/Jaeger-2B2B2B?logo=jaeger&logoColor=66CFE3" alt="Jaeger">
</p>

# ShopAPI 🛍️

یک API فروشگاهی کامل با **ASP.NET Core 9** و **معماری Clean Architecture** که مسیر یک خرید واقعی را پوشش می‌دهد: ثبت‌نام و احراز هویت، مرور محصولات، سبد خرید، تخفیف و کد تخفیف، Checkout با Idempotency Key، مدیریت موجودی انبار، سفارش و پرداخت با درگاه‌های Sandbox (سامان و زرین‌پال).

همراه با زیرساخت قابل مشاهده‌پذیری (Observability) کامل شامل **OpenTelemetry**، **Prometheus**، **Grafana**، **Loki** و **Jaeger** که همه با یک دستور `docker compose up` بالا می‌آیند.

> این پروژه برای یادگیری و تمرین معماری چندلایه و الگوهای واقعی تولیدی (Production-grade) مناسب است.

---

## ✨ ویژگی‌های کلیدی

### 🌱 Seeder اختصاصی و Idempotent
- سیستم Seed ماژولار با ۱۱ زیرسیدر مستقل: `Category`، `Brand`، `Product`، `Discount`، `Coupon`، `User`، `Review`، `Cart`، `Wishlist` و `Order`
- داده‌ها از فایل‌های JSON در `Infrastructure/Persistence/Seed/Data/*.json` خوانده می‌شوند
- **کاملاً Idempotent**: هر زیرسیدر قبل از Insert، وجود رکورد را با کلید طبیعی (Title / Email / SKU / Code و...) بررسی می‌کند و در صورت وجود، از آن صرف‌نظر می‌کند؛ اجرای مکرر کاملاً بی‌خطر است
- اجرای کل Seed درون یک **Transaction** انجام می‌شود؛ در صورت بروز خطا Rollback کامل انجام می‌گیرد
- کنترل از طریق تنظیمات `Seed:Enabled` و `Seed:Force` (فقط در محیط Development) و اجرای خودکار Migration هنگام استارت برنامه

### ⚔️ مدیریت Concurrency
- **Optimistic Concurrency** با `RowVersion` (SQL Server rowversion) روی entityهای حساس: `Product`، `InventoryItem` و `RefreshToken`
- مدیریت `DbUpdateConcurrencyException` در سرویس‌های حساس (Product، Checkout، Inventory و Auth) تا از گم شدن به‌روزرسانی‌ها در درخواست‌های همزمان جلوگیری شود

### 🚦 Rate Limiting
محدودسازی درخواست با Rate Limiter داخلی .NET، به‌صورت **سراسری + چند پالیسی اختصاصی** بر اساس پنجره ثابت (Fixed Window):

| پالیسی | محدودیت | پارتیشن‌بندی |
|---|---|---|
| Global | ۱۰۰ درخواست در دقیقه | بر اساس IP |
| `Auth` | ۵ درخواست در دقیقه | بر اساس IP |
| `RefreshToken` | ۱۰ درخواست در دقیقه | بر اساس IP |
| `Sensitive` | ۱۰ درخواست در دقیقه | بر اساس UserId / IP |
| `Search` | ۶۰ درخواست در دقیقه | بر اساس IP |
| `Write` | ۳۰ درخواست در دقیقه | بر اساس UserId / IP |

در صورت عبور از حد مجاز، پاسخ `429 Too Many Requests` با پیام فارسی و هدر `Retry-After` برگردانده می‌شود.

### ✅ اعتبارسنجی با FluentValidation
- اعتبارسنجی **خودکار** روی ورودی‌های همه کنترلرها (`AutomaticValidationEnabled`)
- Validatorهای مجزا برای هر Feature و هر عملیات (Create/Edit و...)
- پاسخ خطای سفارشی و یکپارچه برای خطاهای Model State

### 🧯 Global Exception Handling
- مدیریت متمرکز خطاها با `IExceptionHandler` و `ProblemDetails`
- نگاشت خودکار Exceptionهای دامنه به Status Code مناسب:
  - `NotFoundException` → `404`
  - `ConflictException` و `InsufficientStockException` → `409`
  - `ForbiddenAccessException` → `403`
  - `BusinessException`، `CartEmptyException`، `InvalidQuantityException` → `400`
- لاگ‌گیری کامل همراه با `TraceId`، `UserId`، `Method` و `Path` برای کورلیشن راحت‌تر در Grafana/Loki

### 🧩 سایر امکانات
- **کش Redis**: لایه کش اختصاصی با `RedisCacheService` و `CacheKeyBuilder` برای لیست، جستجو و جزئیات محصولات + باطل‌سازی کش با Pattern بعد از هر تغییر
- **Idempotency Key**: ذخیره کلید idempotency برای عملیات Checkout تا پرداخت تکراری در اثر Retry کلاینت اتفاق نیفتد
- **احراز هویت JWT**: Access Token + Refresh Token (با انقضا و RowVersion)، هش رمز عبور، فراموشی/بازیابی رمز با ارسال ایمیل SMTP و قالب HTML
- **درگاه‌های پرداخت**: سامان و زرین‌پال (Sandbox) با الگوی Strategy و `PaymentGatewayResolver`
- **Health Checks**: سه endpoint ی `/health`، `/health/live` و `/health/ready` (شامل چک اتصال دیتابیس)
- **Soft Delete** برای محصولات و ثبت تاریخچه `InventoryTransaction` برای موجودی انبار
- **Swagger / OpenAPI** با پشتیبانی کامل از احراز هویت Bearer (دکمه Authorize)

## 🧱 تکنولوژی‌ها

| دسته | تکنولوژی | نسخه |
|---|---|---|
| زبان و پلتفرم | C# 13 / .NET 9 | 9.0 |
| فریم‌ورک | ASP.NET Core Web API | 9.0.17 |
| ORM | Entity Framework Core + SQL Server | 9.0.17 |
| کش | Redis (StackExchange.Redis) | 3.1.31 |
| اعتبارسنجی | FluentValidation | 11.3.1 |
| لاگ‌گیری | Serilog (Console / File / Grafana Loki) | 4.4.0 |
| مستندات | Swagger / Swashbuckle | 9.0.6 |
| Observability | OpenTelemetry | 1.18.0 |
| Metrics | Prometheus | latest |
| Distributed Tracing | Jaeger (OTLP) | latest |
| داشبورد و لاگ | Grafana + Loki | latest |
| کانتینر | Docker Compose | — |

## 🏗 معماری پروژه

```text
ShopAPI/
├── API/              # نقطه ورود، کنترلرها، Middlewareها، Rate Limiter، Health Checks و تنظیمات
├── Application/      # سرویس‌ها، DTOها، Validatorها، قراردادها و لایه Caching
├── Domain/           # Entityها، Enumها و Exceptionهای دامنه
├── Infrastructure/   # EF Core، Migrations، Repositoryها، Seeder، JWT، Hashing، Email و Payment Providerها
├── Shared/           # کلاس‌های مشترک (Exceptionها و...)
└── docker-compose.yml
```

جریان وابستگی‌ها به سمت Domain است: `API → Application → Domain` و `Infrastructure → Application/Domain`؛ قراردادها (Interfaceها) در Application تعریف و در Infrastructure پیاده‌سازی می‌شوند.

## 🚀 راه‌اندازی سریع

### پیش‌نیازها

- [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (یا SQL Server Express / LocalDB)
- Docker Desktop (برای Redis و پشته Observability)

### 1. دریافت پروژه

```bash
git clone https://github.com/NimaHaji/ShopAPI.git
cd ShopAPI
```

### 2. اجرای سرویس‌های جانبی با Docker

```bash
docker compose up -d
```

این دستور این سرویس‌ها را بالا می‌آورد:

| سرویس | آدرس |
|---|---|
| Redis | `localhost:63799` |
| Jaeger UI | `http://localhost:16686` |
| Prometheus | `http://localhost:9090` |
| Loki | `http://localhost:31000` |
| Grafana | `http://localhost:30000` (admin/admin) |

### 3. تنظیمات محلی

```bash
cp API/appsettings.example.json API/appsettings.Development.json
```

سپس این مقادیر را در `API/appsettings.Development.json` مطابق سیستم خودتان تغییر دهید:

- `ConnectionStrings:local` — رشته اتصال SQL Server
- `ConnectionStrings:Redis` — آدرس Redis (پیش‌فرض `localhost:6379`، با docker-compose بالا `localhost:63799`)
- `JwtSettings:SecretKey` — یک کلید طولانی و تصادفی
- تنظیمات Sandbox پرداخت در بخش `Payment`
- `Seed:Enabled` — فعال/غیرفعال کردن داده‌های اولیه
- `Otlp:Endpoint` — آدرس Jaeger (پیش‌فرض `http://localhost:4317`)

> فایل‌های حاوی secret نباید commit شوند؛ `appsettings.example.json` فقط به‌عنوان نمونه در گیت نگه داشته می‌شود.

### 4. اجرای پروژه

```bash
dotnet run --project API
```

- Swagger: `http://localhost:4075/swagger`
- Metrics: `http://localhost:4075/metrics`
- Health: `http://localhost:4075/health`

> هنگام استارت برنامه، Migrationها به‌صورت خودکار اجرا و در صورت فعال بودن `Seed:Enabled`، داده‌های نمونه (محصولات، برندها، دسته‌بندی‌ها، کاربران و...) به‌صورت Idempotent درج می‌شوند.

## 🧪 تست API

1. پروژه و سرویس‌های Docker را اجرا کنید.
2. وارد Swagger شوید و با `POST /api/Users/Register` و `POST /api/Users/login` توکن بگیرید.
3. در Swagger روی دکمه **Authorize** کلیک کنید و مقدار زیر را وارد کنید:

```text
Bearer YOUR_ACCESS_TOKEN
```

مسیرهای اصلی برای تست سناریوی خرید:

```text
GET    /api/Products              # لیست محصولات (کش‌شده در Redis)
GET    /api/Products/Search       # جستجو
POST   /api/Cart/items            # افزودن به سبد خرید
POST   /api/Checkouts/checkout    # ثبت سفارش (با Idempotency Key)
POST   /api/Payments/GetPaymentUrl# دریافت لینک پرداخت (سامان / زرین‌پال)
POST   /api/Coupons/...           # کد تخفیف
```

## 🔍 Observability

پس از اجرای پروژه و `docker compose up`:

- **متریک‌ها**: endpoint `/metrics` توسط Prometheus هر ۵ ثانیه scrape می‌شود (تنظیمات در `prometheus.yml`)
- **تریس‌ها**: Distributed Tracing با OpenTelemetry (ASP.NET Core، EF Core، HttpClient) از طریق OTLP به Jaeger ارسال و در `http://localhost:16686` قابل مشاهده است
- **لاگ‌ها**: Serilog به‌صورت همزمان در Console، فایل‌های رولینگ (`API/Logs/`) و Grafana Loki می‌نویسد
- **داشبورد**: Grafana در `http://localhost:30000` برای ساخت داشبورد متریک و لاگ (با کورلیشن TraceId)

## ⚠️ وضعیت پروژه

این پروژه آموزشی و در حال توسعه است. تست‌های خودکار هنوز اضافه نشده‌اند و ممکن است edge caseهایی پوشش داده نشده باشند. اگر قصد استفاده جدی دارید، تنظیمات امنیتی و HTTPS را کامل‌تر کنید.

## 🤝 مشارکت

اگر این پروژه برایتان مفید بود، می‌توانید آن را Fork کنید و روی بخش‌هایی مثل تست‌نویسی، الگوهای CQRS، تولید کد یا بهبود Observability تمرین کنید. از PR ها با حفظ معماری استقبال می شود .
