# ShopAPI

یک API فروشگاهی ساده و آموزشی با ASP.NET Core است که برای تمرین معماری چندلایه، احراز هویت JWT، کار با Entity Framework Core، سبد خرید، سفارش و پرداخت sandbox ساخته شده است.

> این پروژه برای یادگیری و تمرین مناسب است؛ اگر قصد استفاده جدی یا production دارید، حتماً تنظیمات امنیتی، لاگینگ، تست‌ها و مدیریت خطا را کامل‌تر کنید.

## ✨ امکانات

- ثبت‌نام، ورود، خروج، پروفایل و refresh token
- احراز هویت و سطح دسترسی با JWT و Role
- مدیریت سبد خرید کاربران
- ساخت سفارش از روی سبد خرید
- ساخت پرداخت و callback برای درگاه‌های sandbox
- seed اولیه محصولات از DummyJSON
- Swagger UI برای تست سریع API
- ساختار چندلایه بر پایه `Domain`، `Application`، `Infrastructure` و `API`

## 🧱 تکنولوژی‌ها

- `.NET 9`
- `ASP.NET Core Web API`
- `Entity Framework Core`
- `SQL Server`
- `JWT Bearer Authentication`
- `FluentValidation`
- `Swagger / OpenAPI`

## 📁 ساختار پروژه

```text
ShopAPI/
├── API/                  # نقطه ورود برنامه، کنترلرها، Swagger و تنظیمات اجرا
├── Application/          # سرویس‌ها، DTOها، اعتبارسنجی‌ها و قراردادهای برنامه
├── Domain/               # Entityها، Enumها و Exceptionهای دامنه
├── Infrastructure/       # EF Core، Repositoryها، Seed، JWT، Hashing و Payment Providerها
├── ShopApi.sln
└── README.md
```

## 🚀 راه‌اندازی سریع

### پیش‌نیازها

- نصب `.NET SDK 9`
- نصب `SQL Server` یا `SQL Server Express`
- دسترسی اینترنت برای seed اولیه محصولات از `https://dummyjson.com`

### 1. دریافت پروژه

```bash
git clone https://github.com/YOUR_USERNAME/ShopAPI.git
cd ShopAPI
```

### 2. تنظیمات محلی

یک فایل محلی بسازید:

```bash
cp API/appsettings.example.json API/appsettings.Development.json
```

سپس مقدارهای زیر را در `API/appsettings.Development.json` مطابق سیستم خودتان تغییر دهید:

- `ConnectionStrings:local`
- `JwtSettings:SecretKey`
- تنظیمات sandbox پرداخت در بخش `Payment`

> فایل‌های local، secret و environment نباید داخل Git commit شوند.

### 3. Restore و Build

```bash
dotnet restore
dotnet build
```

### 4. اجرای پروژه

```bash
dotnet run --project API
```

بعد از اجرا، Swagger معمولاً از یکی از آدرس‌های زیر در دسترس است:

```text
http://localhost:5033/swagger
https://localhost:7033/swagger
```

> در زمان شروع برنامه، migrationها به‌صورت خودکار اجرا می‌شوند و اگر جدول محصولات خالی باشد، داده‌های نمونه seed می‌شوند.

## 🔐 نکات امنیتی برای Open Source

برای اینکه پروژه تمیز و امن روی GitHub منتشر شود:

- هیچ `ConnectionString` واقعی، کلید JWT، token، رمز عبور یا اطلاعات درگاه واقعی را commit نکنید.
- از `API/appsettings.example.json` به‌عنوان نمونه استفاده کنید و تنظیمات واقعی را فقط در فایل‌های local نگه دارید.
- فایل‌هایی مثل `.env`، `appsettings.Development.json`، `appsettings.Local.json`، `secrets.json`، `bin/` و `obj/` باید ignore باشند.
- اگر قبلاً secret واقعی commit شده، فقط حذف از فایل کافی نیست؛ باید secret را rotate کنید و در صورت نیاز history گیت را پاک‌سازی کنید.

## 🧪 تست API

برای شروع ساده:

1. پروژه را اجرا کنید.
2. وارد Swagger شوید.
3. از endpoint ثبت‌نام و ورود، token بگیرید.
4. در Swagger روی `Authorize` کلیک کنید و مقدار زیر را وارد کنید:

```text
Bearer YOUR_ACCESS_TOKEN
```

نمونه مسیرهای مهم:

- `POST /api/User/Register`
- `POST /api/User/login`
- `GET /api/User/Profile`
- `GET /api/Cart/Cart`
- `POST /api/Cart/items`
- `POST /api/Checkout/checkout`
- `POST /api/Payment/GetPaymentUrl`

## 🧭 نقشه راه

در مراحل بعدی قرار است این بخش‌ها اضافه یا کامل‌تر شوند:

- اضافه شدن ماژول انبارداری برای کم و زیاد شدن تعداد محصولات و مدیریت دقیق‌تر موجودی
- اضافه شدن Docker و Docker Compose تا بقیه بتوانند پروژه را راحت‌تر برای تمرین بالا بیاورند
- بهبود تست‌ها و سناریوهای خطا
- کامل‌تر شدن مستندات endpointها
- تمیزکاری بیشتر برای آماده‌سازی production

## 🤝 مناسب برای تمرین

اگر می‌خواهید یک API فروشگاهی ساده را تمرین کنید، این پروژه می‌تواند نقطه شروع خوبی باشد. می‌توانید آن را fork کنید و روی بخش‌هایی مثل product management، order flow، payment، authentication یا clean architecture تمرین کنید.

## ⚠️ وضعیت پروژه

این پروژه آموزشی و در حال توسعه است. ممکن است باگ‌های خیلی ریز، edge caseهای پوشش‌داده‌نشده یا قسمت‌هایی که هنوز نیاز به refactor دارند داخل آن وجود داشته باشد.

