# Backend Setup

Follow these steps before starting backend development.

1. Accept the GitHub invitation in your email.

2. Open terminal and run:

```powershell
git clone https://github.com/Subham-s0/AutoPartSystem_Backend.git
cd AutoPartSystem_Backend
git checkout main
git pull origin main
git checkout -b YourName
git push -u origin YourName
```

Example:

```powershell
git checkout -b subham
git push -u origin subham
```

3. Open the solution in Visual Studio:

```text
Backend/VehiStock/VehiStock.slnx
```

NuGet packages should restore automatically.

4. Create a PostgreSQL database called:

```text
VehiStockDb
```

You can create it in pgAdmin, or let EF create it during `database update`.

5. Set up your local configurations:

- Copy `Backend/VehiStock/appsettings.local.Example.json` and rename it to `Backend/VehiStock/appsettings.local.json`.

- Open your new `Backend/VehiStock/appsettings.local.json` and update the database password in the connection string:
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=VehiStockDb;Username=postgres;Password=YOUR_DATABASE_PASSWORD"
  }
  ```
  Replace `YOUR_DATABASE_PASSWORD` with your local PostgreSQL password. You can also configure your own local email settings or Khalti/Google API keys here if needed.

- **Note**: `appsettings.local.json` is gitignored and will not be pushed to Git. Always use this file for your local credentials so they are not overwritten when pulling from `main` or merging branches.

6. Apply the database migrations:

```powershell
cd Backend
dotnet ef database update `
  --project ".\VehiStock.Infrastructure\VehiStock.Infrastructure.csproj" `
  --startup-project ".\VehiStock\VehiStock.csproj"
```

7. Start coding in your own branch and push only to your branch.
