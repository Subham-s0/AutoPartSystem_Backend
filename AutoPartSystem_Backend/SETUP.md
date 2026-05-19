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

5. Update the PostgreSQL connection string in:

```text
Backend/VehiStock/appsettings.json
```

Find:

```text
YOUR_PASSWORD_HERE
```

and replace it with your local PostgreSQL password.

6. Apply the database migrations:

```powershell
cd Backend
dotnet ef database update `
  --project ".\VehiStock.Infrastructure\VehiStock.Infrastructure.csproj" `
  --startup-project ".\VehiStock\VehiStock.csproj"
```

7. Start coding in your own branch and push only to your branch.
