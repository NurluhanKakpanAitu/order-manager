# OrderManager - Система управления заказами интернет-магазина

## Описание проекта

OrderManager - это система управления заказами для интернет-магазина, реализованная с использованием Clean Architecture. Система предоставляет REST API для:

- CRUD операций с товарами и категориями
- Создания и управления заказами
- Поиска и фильтрации товаров
- Обработки оплаты заказов

## Архитектура

Проект построен по принципам Clean Architecture и состоит из следующих слоев:

- **Domain** - доменные сущности, интерфейсы и бизнес-логика
- **Application** - сервисы приложения, DTOs и интерфейсы сервисов  
- **Infrastructure** - репозитории, Entity Framework, внешние сервисы
- **API** - Web API контроллеры и конфигурация

### Ключевые паттерны

- **Generic Repository Pattern** - универсальный репозиторий для базовых CRUD операций
- **Unit of Work Pattern** - управление транзакциями и группировка операций
- **Dependency Injection** - инверсия зависимостей
- **CQRS-подобный подход** - разделение команд и запросов

## Структура проекта

```
OrderManager/
├── Core/
│   ├── Domain/                 # Доменный слой
│   │   ├── Entities/          # Сущности
│   │   ├── Enums/             # Перечисления
│   │   ├── Interfaces/        # Интерфейсы репозиториев
│   │   └── ValueObjects/      # Объекты-значения
│   └── Application/           # Прикладной слой
│       ├── DTOs/              # Объекты передачи данных
│       ├── Interfaces/        # Интерфейсы сервисов
│       └── Services/          # Сервисы приложения
├── Infrastructure/
│   └── Infrastructure.Persistence/  # Слой инфраструктуры
│       ├── Data/              # DbContext
│       ├── Configurations/    # Конфигурации EF
│       ├── Repositories/      # Реализации репозиториев
│       └── Services/          # Внешние сервисы
├── API/
│   └── WebAPI/                # Web API
│       └── Controllers/       # Контроллеры
└── Tests/
    └── OrderManager.Tests/    # Модульные тесты
```

## Основные сущности

### Product (Товар)
- Id, Name (многоязычное), Description (многоязычное)
- Price, Quantity, CategoryId
- CreatedAt, UpdatedAt

### Category (Категория)
- Id, Name (многоязычное), Description (многоязычное)
- CreatedAt, UpdatedAt

### Order (Заказ)
- Id, TotalAmount, Status (New/Paid/Cancelled)
- Items (список товаров в заказе)
- CreatedAt, UpdatedAt

### OrderItem (Элемент заказа)
- Id, ProductId, Quantity, Price

## API Endpoints

### Categories
- `GET /api/categories` - получить все категории
- `GET /api/categories/{id}` - получить категорию по ID
- `POST /api/categories` - создать категорию
- `PUT /api/categories/{id}` - обновить категорию
- `DELETE /api/categories/{id}` - удалить категорию

### Products
- `GET /api/products` - получить все товары
- `GET /api/products/search` - поиск товаров с фильтрами
- `GET /api/products/{id}` - получить товар по ID
- `POST /api/products` - создать товар
- `PUT /api/products/{id}` - обновить товар
- `DELETE /api/products/{id}` - удалить товар

### Orders
- `GET /api/orders` - получить все заказы
- `GET /api/orders/by-status/{status}` - получить заказы по статусу
- `GET /api/orders/{id}` - получить заказ по ID
- `POST /api/orders` - создать заказ
- `POST /api/orders/{id}/pay` - оплатить заказ
- `POST /api/orders/{id}/cancel` - отменить заказ

## Поиск и фильтрация товаров

Endpoint `GET /api/products/search` поддерживает следующие параметры:
- `searchTerm` - поиск по названию (подстрока)
- `categoryId` - фильтр по категории
- `minPrice` - минимальная цена
- `maxPrice` - максимальная цена

## Бизнес-правила

1. **Заказы**:
   - При создании заказ получает статус "New"
   - Оплата и отмена возможны только для заказов со статусом "New"
   - При создании заказа количество товаров уменьшается
   - При отмене заказа количество товаров восстанавливается

2. **Товары**:
   - Количество не может быть отрицательным
   - При заказе проверяется наличие достаточного количества

3. **Категории**:
   - Нельзя удалить категорию, содержащую товары

## Запуск проекта

### Предварительные требования
- .NET 8.0 SDK
- Visual Studio 2022 или VS Code

### Команды для запуска

1. Клонируйте репозиторий и перейдите в директорию проекта

2. Восстановите зависимости:
```bash
dotnet restore
```

3. Соберите проект:
```bash
dotnet build
```

4. Запустите тесты:
```bash
dotnet test Tests/OrderManager.Tests/OrderManager.Tests.csproj
```

5. Запустите Web API:
```bash
dotnet run --project API/WebAPI/WebAPI.csproj
```

6. Откройте Swagger UI: `http://localhost:5175/swagger`

## Тестирование API

### Postman
Импортируйте коллекцию из файла `OrderManager_API_Collection.postman_collection.json`

### Swagger
После запуска API доступен по адресу: `http://localhost:5175/swagger`

## Примеры использования

### 1. Создание категории
```http
POST /api/categories
Content-Type: application/json

{
  "nameKz": "Электроника",
  "nameRu": "Электроника", 
  "nameEn": "Electronics",
  "descriptionKz": "Электронные товары",
  "descriptionRu": "Электронные товары",
  "descriptionEn": "Electronic goods"
}
```

### 2. Создание товара
```http
POST /api/products
Content-Type: application/json

{
  "nameKz": "Смартфон",
  "nameRu": "Смартфон",
  "nameEn": "Smartphone",
  "descriptionKz": "Современный смартфон",
  "descriptionRu": "Современный смартфон", 
  "descriptionEn": "Modern smartphone",
  "price": 599.99,
  "quantity": 50,
  "categoryId": "{category-id}"
}
```

### 3. Создание заказа
```http
POST /api/orders
Content-Type: application/json

{
  "items": [
    {
      "productId": "{product-id}",
      "quantity": 2
    }
  ]
}
```

### 4. Оплата заказа
```http
POST /api/orders/{order-id}/pay
```

## 🐳 Docker Support

### База данных
Проект поддерживает PostgreSQL через Docker:

```bash
# Запуск только PostgreSQL + PgAdmin
docker-compose -f docker-compose.postgres.yml up -d

# Полное приложение в Docker
docker-compose up -d
```

### Доступ к базе данных
- **PostgreSQL**: localhost:5432 (username: ordermanager, password: OrderManager123!)
- **PgAdmin**: http://localhost:8081 (admin@ordermanager.com / admin123)

Подробнее в [DOCKER.md](DOCKER.md)

## Технологии

- **.NET 8.0** - основной фреймворк
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **PostgreSQL** - основная база данных
- **In-Memory Database** - для тестирования и разработки
- **Docker & Docker Compose** - контейнеризация
- **Swagger/OpenAPI** - документация API
- **xUnit** - фреймворк тестирования
- **Moq** - мокирование для тестов
- **FluentAssertions** - assertions для тестов

## Покрытие тестами

Проект включает модульные тесты для:
- Доменных сущностей (Product, Order)
- Сервисов приложения (ProductService, OrderService)
- Бизнес-логики и валидации

Для запуска тестов:
```bash
dotnet test
```

## Статусы заказов

- **0 (New)** - новый заказ
- **1 (Paid)** - оплаченный заказ
- **2 (Cancelled)** - отмененный заказ

## Payment Service

Реализован упрощенный сервис оплаты, который:
- Симулирует обработку платежей с вероятностью успеха 90%
- Логирует результаты операций в консоль
- Поддерживает операции оплаты и возврата

## Многоязычность

Система поддерживает три языка:
- **kz** - казахский
- **ru** - русский  
- **en** - английский (по умолчанию)

Названия и описания хранятся в объекте Translation с поддержкой всех языков.