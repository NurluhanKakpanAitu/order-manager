# Docker Setup для OrderManager

## 🐳 Запуск PostgreSQL

### Вариант 1: Только PostgreSQL + PgAdmin
```bash
docker-compose -f docker-compose.postgres.yml up -d
```

### Вариант 2: Полное приложение с PostgreSQL
```bash
docker-compose up -d
```

## 🔧 Конфигурация

### PostgreSQL Database
- **Host**: localhost
- **Port**: 5433 (для postgres-only) или 5432 (для full-stack)
- **Database**: OrderManagerDb
- **Username**: ordermanager
- **Password**: OrderManager123!

### PgAdmin
- **URL**: http://localhost:8081
- **Email**: admin@ordermanager.com
- **Password**: admin123

## 🚀 Команды Docker

### Запуск контейнеров
```bash
# Только PostgreSQL
docker-compose -f docker-compose.postgres.yml up -d

# Полное приложение
docker-compose up -d

# Просмотр логов
docker-compose logs -f

# Остановка контейнеров
docker-compose down

# Остановка с удалением данных
docker-compose down -v
```

### Проверка статуса
```bash
# Список контейнеров
docker ps

# Логи конкретного контейнера
docker logs ordermanager-postgres
docker logs ordermanager-api
```

## 🛠 Разработка

### Локальная разработка с PostgreSQL в Docker
1. Запустите только PostgreSQL:
   ```bash
   docker-compose -f docker-compose.postgres.yml up -d
   ```

2. Обновите connection string в `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5433;Database=OrderManagerDb;Username=ordermanager;Password=OrderManager123!"
     }
   }
   ```

3. Запустите приложение локально:
   ```bash
   dotnet run --project API/WebAPI
   ```

### Полная контейнеризация
```bash
# Собрать и запустить все в Docker
docker-compose up --build -d

# API будет доступно на http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

## 🔄 Миграции базы данных

### Автоматическое применение миграций
Миграции применяются автоматически при старте приложения.

### Ручное применение миграций
```bash
# Создание новой миграции
dotnet ef migrations add MigrationName -p Infrastructure/Infrastructure.Persistence -s API/WebAPI

# Применение миграций
dotnet ef database update -p Infrastructure/Infrastructure.Persistence -s API/WebAPI
```

## 📝 Troubleshooting

### Порт уже используется
Если порт 5432 занят другим PostgreSQL:
```bash
# Остановить все PostgreSQL контейнеры
docker stop $(docker ps -q -f ancestor=postgres)

# Или изменить порт в docker-compose.postgres.yml на 5433:5432
```

### Проблемы с подключением к базе
1. Проверьте, что контейнер запущен:
   ```bash
   docker ps | grep postgres
   ```

2. Проверьте логи:
   ```bash
   docker logs ordermanager-postgres
   ```

3. Проверьте connection string в настройках приложения

### Сброс базы данных
```bash
# Остановить контейнеры и удалить данные
docker-compose down -v

# Запустить заново
docker-compose up -d
```

## 🌐 URLs

- **API**: http://localhost:5000 (в Docker) или http://localhost:5175 (локально)
- **Swagger UI**: http://localhost:5000/swagger
- **PgAdmin**: http://localhost:8081
- **PostgreSQL**: localhost:5432 (или 5433)