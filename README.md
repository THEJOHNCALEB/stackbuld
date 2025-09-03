# Stackbuld Product Ordering API

A production-grade .NET 8 Web API built with Clean Architecture principles for product catalog management and order processing with concurrency-safe stock management.

## Features

- **Product Catalog Management**: Full CRUD operations for products
- **Order Processing**: Place orders with multiple products
- **Stock Management**: Concurrency-safe stock updates to prevent overselling
- **Clean Architecture**: Separation of concerns with Domain, Application, Infrastructure, and API layers
- **Database Transactions**: Atomic operations for data integrity
- **API Documentation**: Swagger/OpenAPI integration
- **Docker Support**: Containerized application with PostgreSQL

## Tech Stack

- **.NET 8** - Latest LTS version
- **ASP.NET Core Web API** - Web API framework
- **Entity Framework Core 8** - ORM with PostgreSQL provider
- **PostgreSQL** - Relational database
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Input validation
- **Docker** - Containerization
- **Swagger/OpenAPI** - API documentation

## Architecture

The solution follows Clean Architecture principles with the following layers:

```
stackbuld/
├── src/                                    # Source code
│   ├── Stackbuld.ProductOrdering.Domain/   # Domain entities and business logic
│   ├── Stackbuld.ProductOrdering.Application/ # Use cases, DTOs, and interfaces
│   ├── Stackbuld.ProductOrdering.Infrastructure/ # Data access and external services
│   └── Stackbuld.ProductOrdering.Api/      # Web API controllers and configuration
├── tests/                                  # Unit and integration tests
├── docker-compose.yml                      # Development environment
├── docker-compose.prod.yml                 # Production environment
├── docker-compose.override.yml             # Local development overrides
├── Dockerfile                              # API container
├── Dockerfile.migrations                   # Database migrations container
├── env.example                             # Environment variables template
├── .gitignore                              # Git ignore rules
└── README.md                               # This file
```

### Domain Layer
- **Entities**: Product, Order, OrderItem
- **Exceptions**: Custom business exceptions
- **Business Logic**: Stock management, order validation

### Application Layer
- **Commands/Queries**: CQRS pattern with MediatR
- **DTOs**: Data transfer objects
- **Interfaces**: Repository contracts
- **Validators**: FluentValidation rules

### Infrastructure Layer
- **DbContext**: Entity Framework configuration
- **Repositories**: Data access implementations
- **Unit of Work**: Transaction management

### API Layer
- **Controllers**: RESTful endpoints
- **Filters**: Global exception handling
- **Configuration**: Dependency injection setup

## Quick Start

### Prerequisites

- .NET 8 SDK
- Docker and Docker Compose
- PostgreSQL (if running locally without Docker)
- Make (optional, for convenience commands)

### Option 1: Docker Compose (Recommended)

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd stackbuld
   ```

2. **Quick setup (using Make)**
   ```bash
   make setup
   ```

   **Or manual setup:**
   ```bash
   # Copy environment file
   cp env.example .env
   
   # Start services
   docker-compose up -d
   
   # Run migrations
   docker-compose up migrations
   ```

3. **Access the API**
   - API: http://localhost:5001
   - Swagger UI: http://localhost:5001/swagger
   - pgAdmin: http://localhost:5050 (admin@stackbuld.com / admin123)

### Option 1.5: Using Make Commands

```bash
# Start development environment
make dev

# View logs
make logs-api

# Check health
make health

# Stop services
make docker-stop

# See all available commands
make help
```

### Option 2: Local Development

1. **Start PostgreSQL**
   ```bash
   # Using Docker
   docker run --name postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:15-alpine
   ```

2. **Update connection string** in `appsettings.Development.json`

3. **Restore packages**
   ```bash
   dotnet restore
   ```

4. **Run database migrations**
   ```bash
   cd src/Stackbuld.ProductOrdering.Api
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

## API Endpoints

### Products

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | Get all products |
| GET | `/api/products/{id}` | Get product by ID |
| POST | `/api/products` | Create new product |
| PUT | `/api/products/{id}` | Update product |
| DELETE | `/api/products/{id}` | Delete product |

### Orders

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/orders` | Get all orders |
| GET | `/api/orders/{id}` | Get order by ID |
| POST | `/api/orders` | Place new order |

## Concurrency Safety

The API implements several mechanisms to prevent overselling:

1. **Database Transactions**: All order operations are wrapped in transactions
2. **Atomic Stock Updates**: Uses raw SQL for atomic stock quantity updates
3. **Optimistic Concurrency**: Stock validation before and during order processing
4. **Rollback on Failure**: Automatic rollback if any product has insufficient stock

### Example Stock Update Query
```sql
UPDATE "Products" 
SET "StockQuantity" = "StockQuantity" - @quantity, "UpdatedAt" = @timestamp 
WHERE "Id" = @productId AND "StockQuantity" >= @quantity
```

## Example Usage

### Create a Product
```bash
curl -X POST "http://localhost:5000/api/products" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 999.99,
    "stockQuantity": 10
  }'
```

### Place an Order
```bash
curl -X POST "http://localhost:5000/api/orders" \
  -H "Content-Type: application/json" \
  -d '{
    "items": [
      {
        "productId": "product-guid-here",
        "quantity": 2
      }
    ]
  }'
```

## Testing

### Running Tests
```bash
dotnet test
```

### Concurrency Test
The solution includes a concurrency test that simulates multiple orders competing for the same stock to verify the system prevents overselling.

## Docker Configuration

The `docker-compose.yml` includes:
- **PostgreSQL 15**: Database service
- **API Service**: .NET 8 Web API
- **Health Checks**: Database connectivity verification
- **Volume Persistence**: Database data persistence

## Assumptions

1. **Single Database**: All operations use a single PostgreSQL database
2. **No Authentication**: API endpoints are publicly accessible (for demo purposes)
3. **Synchronous Processing**: Orders are processed synchronously
4. **No Caching**: No caching layer implemented
5. **No Logging**: Basic logging configuration only

## Configuration

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Development/Production
- `ConnectionStrings__DefaultConnection`: Database connection string

### Database Configuration
- **Provider**: PostgreSQL
- **Migrations**: Entity Framework Core migrations
- **Connection Pooling**: Default EF Core connection pooling

## License

This project is licensed under the MIT License.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## Support

For questions or support, please open an issue in the repository.
