# Primitive Clash Backend

Backend de **Primitive Clash**, que expone una API web y utiliza PostgreSQL como base de datos y Redis como sistema de caché.

Este proyecto se puede levantar de forma rápida y consistente usando **Docker**.

---

## Requisitos

- Docker >= 24
- Docker Compose >= 2.0

---

## Levantar el proyecto con Docker

El proyecto incluye un `Dockerfile` y un `docker-compose.yml` que levantan:

- PostgreSQL (`postgres:15`)
- Redis (`redis:7`)
- API web (`primitive-clash-backend`)

---

### 1️⃣ Construir y levantar los servicios

```bash
docker-compose up --build -d
```

- `-d` ejecuta los contenedores en **segundo plano**.

---

### 2️⃣ Acceder a la API

- URL base: http://localhost:5247
- Puerto 5247 del host está mapeado al puerto 80 del contenedor web.

---

### 3️⃣ Variables de entorno relevantes

Las variables ya están configuradas en `docker-compose.yml`:

- ConnectionStrings\_\_DefaultConnection → apunta al servicio `postgres`
- Redis\_\_ConnectionString → apunta al servicio `redis:6379`

> Nota: Puedes sobrescribirlas creando un archivo `.env` en la raíz del proyecto.

---

### 4️⃣ Detener y eliminar servicios

```bash
docker-compose down -v
```

- `-v` elimina **volúmenes persistentes**, incluyendo la base de datos.
- Si quieres **mantener los datos**, omite `-v`.

---

### 5️⃣ Migraciones y seeding

Al levantar la API:

- Se aplican automáticamente todas las **migraciones pendientes**.
- Se ejecuta DbSeeder para insertar datos iniciales (cartas y arenas).
- El seeding es **idempotente**, por lo que no sobrescribe datos existentes.

---

## Tests y Coverage

### Ejecutar todas las pruebas

```bash
dotnet test primitive-clash-backend.sln
```

### Ver solo las pruebas que fallan

```powershell
dotnet test primitive-clash-backend.sln --logger "console;verbosity=normal" | Select-String -Pattern "error TESTERROR|FAIL" -Context 1,0
```

### Ejecutar pruebas con cobertura de código

```bash
dotnet test primitive-clash-backend.sln --collect:"XPlat Code Coverage"
```

### Generar reporte HTML de coverage

```powershell
.\generate-coverage.ps1
```

Este script ejecuta las pruebas, genera el reporte de cobertura y abre automáticamente el resultado en el navegador.

---

Con esto, cualquier desarrollador puede levantar el proyecto de forma consistente en su máquina local.
