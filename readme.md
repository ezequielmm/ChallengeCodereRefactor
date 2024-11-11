# ⛽️ ChallengeCodereRefactor Project

![.NET](https://img.shields.io/badge/.NET-6.0-blue.svg)
![Build](https://img.shields.io/badge/build-passing-brightgreen.svg)

## 💑 Introduction

Welcome to the **ChallengeCodereRefactor Project**! This project is a web application built with **ASP.NET Core** using **Domain-Driven Design (DDD)** principles. It manages TV shows, allowing users to perform CRUD operations, fetch data from an external API, and more.

## ✨ Features

- **CRUD Operations**: Manage TV shows, networks, genres, and ratings.
- **External API Integration**: Fetch and store shows from an external API.
- **Swagger Documentation**: Interactive API documentation.
- **Unit & Integration Tests**: Ensure code quality and reliability.
- **Entity Framework Core**: Manage database interactions.

## 🫠 Architecture & Patterns

### 📦 Layers

1. **Domain**: 
   - Business logic and domain entities.
   - **Entities**: Core business objects (e.g., Show, Network).
   - **Repositories Interfaces**: Contracts for data access.

2. **Application**: 
   - Application logic and use cases.
   - **DTOs**: Data Transfer Objects.
   - **Services Interfaces**: Application service contracts.

3. **Infrastructure**: 
   - Technical details and external systems.
   - **Persistence**: Repository implementations using EF Core.

4. **UI**: 
   - API Controllers and Swagger integration.

5. **Tests**: 
   - **Unit Tests**: Test individual components.
   - **Integration Tests**: Test component interactions.

### 🔄 Design Patterns

- **Repository Pattern**
- **Dependency Injection**
- **DTO Pattern**

## 🚀 Getting Started

### 🔧 Prerequisites

- **[Visual Studio 2022](https://visualstudio.microsoft.com/vs/)**
- **[.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)**
- **[SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)**
- **[Git](https://git-scm.com/downloads)**

### 📅 Installation

1. **Clone the Repository**

   ```bash
   git clone https://github.com/ezequielmm/ChallengeCodereRefactor.git
   ```

2. **Navigate to the Project Directory**

   ```bash
   cd ChallengeCodereRefactor
   ```

3. **Restore Dependencies**

   ```bash
   dotnet restore
   ```

## 💄️ Database Setup

### 📜 Creating Migrations

```bash
dotnet ef migrations add InitialCreate
```

### ⚒️ Applying Migrations

```bash
dotnet ef database update
```

## 🏃 Running the Project

### 💻 From Visual Studio

- Press `F5` or `Ctrl + F5`.

### 🔡 From Command Line

```bash
cd src/Configurations
```
Run the ISS EXPRESS Profile in Visual Studio.

## 🔤 Using Swagger

Access Swagger UI at:

```https://localhost:44330/swagger/index.html```

## 🧪 Running Tests

### 🔬 Unit Tests

```bash
cd src/Tests/ApplicationTests
dotnet test
```

### 🔍 Integration Tests

```bash
cd src/Tests/Integration
dotnet test
```

## 📣 Contact

- **Email**: ezequielmora1986@gmail.com
- **GitHub**: [@ezequielmm](https://github.com/ezequielmm)