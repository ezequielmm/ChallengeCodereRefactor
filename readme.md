```markdown
# 📺 ChallengeCodereRefactor Project

![.NET](https://img.shields.io/badge/.NET-6.0-blue.svg)
![Build](https://img.shields.io/badge/build-passing-brightgreen.svg)

## 📑 Table of Contents

- [📖 Introduction](#-introduction)
- [✨ Features](#-features)
- [🧰 Architecture & Patterns](#-architecture--patterns)
- [🚀 Getting Started](#-getting-started)
  - [🔧 Prerequisites](#-prerequisites)
  - [📥 Installation](#-installation)
- [🗄️ Database Setup](#️-database-setup)
  - [📜 Creating Migrations](#-creating-migrations)
  - [🛠️ Applying Migrations](#️-applying-migrations)
- [🏃 Running the Project](#-running-the-project)
  - [💻 From Visual Studio](#-from-visual-studio)
  - [🖥️ From Command Line](#️-from-command-line)
- [🖥️ Using Swagger](#️-using-swagger)
- [🧪 Running Tests](#-running-tests)
  - [🔬 Unit Tests](#-unit-tests)
  - [🔍 Integration Tests](#-integration-tests)
- [🤝 Contributing](#-contributing)
- [📜 License](#-license)
- [📫 Contact](#-contact)

## 📖 Introduction

Welcome to the **ChallengeCodereRefactor Project**! 🎉 This project is a web application built with **ASP.NET Core** using **Domain-Driven Design (DDD)** principles. It manages TV shows, allowing users to perform CRUD (Create, Read, Update, Delete) operations, fetch data from an external API, and more.

Whether you're a seasoned developer or just starting out, this guide will help you understand, set up, and run the project effortlessly. Let's dive in! 🚀

## ✨ Features

- **CRUD Operations**: Manage TV shows, networks, genres, and ratings.
- **External API Integration**: Fetch and store shows from an external API.
- **Swagger Documentation**: Interactive API documentation for easy testing.
- **Unit & Integration Tests**: Ensure code quality and reliability.
- **Entity Framework Core**: Manage database interactions seamlessly.

## 🧰 Architecture & Patterns

This project follows the **Domain-Driven Design (DDD)** architecture, ensuring a clear separation of concerns and maintainable codebase.

### 📦 Layers

1. **Domain**: 
   - Contains business logic and domain entities.
   - **Entities**: Core business objects (e.g., Show, Network).
   - **Repositories Interfaces**: Contracts for data access.

2. **Application**: 
   - Handles application logic and use cases.
   - **DTOs**: Data Transfer Objects for communication.
   - **Services Interfaces**: Contracts for application services.
   - **Services**: Implementations of application services.

3. **Infrastructure**: 
   - Deals with technical details and external systems.
   - **Persistence**: Repository implementations using Entity Framework Core.
   - **Services**: External service integrations.
   - **Data**: Database context and configurations.

4. **UI**: 
   - API Controllers and Swagger integration.
   - **Controllers**: Handle HTTP requests and responses.

5. **Tests**: 
   - **Unit Tests**: Test individual components.
   - **Integration Tests**: Test interactions between components.

### 🔄 Design Patterns

- **Repository Pattern**: Abstracts data access logic.
- **Dependency Injection**: Promotes loose coupling and easier testing.
- **DTO Pattern**: Transfers data between layers without exposing internal structures.

## 🚀 Getting Started

Follow these steps to get the project up and running on your local machine.

### 🔧 Prerequisites

Before you begin, ensure you have the following installed:

- **[Visual Studio 2022](https://visualstudio.microsoft.com/vs/)** or later with **.NET Core** development tools.
- **[.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)**
- **[SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)** (for local database)
- **[Git](https://git-scm.com/downloads)**

### 📥 Installation

1. **Clone the Repository**

   ```bash
   git clone https://github.com/ezequielmm/ChallengeCodereRefactor.git
   ```

2. **Navigate to the Project Directory**

   ```bash
   cd ChallengeCodereRefactor
   ```

3. **Restore Dependencies**

   Open the solution in Visual Studio and it will automatically restore NuGet packages. Alternatively, use the command line:

   ```bash
   dotnet restore
   ```

## 🗄️ Database Setup

Setting up the database involves creating migrations and applying them to create the necessary tables.

### 📜 Creating Migrations

Migrations allow you to update your database schema based on changes in your models.

1. **Open Terminal in Project Directory**

2. **Run the Migration Command**

   ```bash
   dotnet ef migrations add InitialCreate
   ```

   - **`InitialCreate`**: Name of the migration. You can choose any descriptive name.

### 🛠️ Applying Migrations

Apply the migrations to create/update the database schema.

1. **Run the Update Command**

   ```bash
   dotnet ef database update
   ```

   This command creates the database (if it doesn't exist) and applies all pending migrations.

## 🏃 Running the Project

You can run the project using **Visual Studio** or the **Command Line Interface (CLI)**.

### 💻 From Visual Studio

1. **Open the Solution**

   Double-click the `ChallengeCodereRefactor.sln` file to open the solution in Visual Studio.

2. **Set Startup Project**

   Ensure that the `Challenge` project is set as the startup project.

3. **Run the Project**

   - Press `F5` to run with debugging.
   - Press `Ctrl + F5` to run without debugging.

   Visual Studio will build the project and launch it in your default browser.

### 🖥️ From Command Line

1. **Navigate to the Project Directory**

   ```bash
   cd src/Configurations
   ```

2. **Run the Project**

   Enter to visual studio 2022 and run the ISS EXPRESS Profile.

## 🖥️ Using Swagger

**Swagger** provides an interactive interface to test and explore your API endpoints.

1. **Access Swagger UI**

   Once the project is running, navigate to:

   ```
   https://localhost:44330/swagger/index.html
   ```

   or

   ```
   http://localhost:44330/swagger/index.html
   ```

2. **Explore Endpoints**

   - You'll see a list of all available API endpoints.
   - Click on an endpoint to expand its details.
   - Use the **"Try it out"** button to test the endpoint directly from the browser.

3. **Authentication**

   - Some endpoints may require an API key.
   - Use the **"Authorize"** button in Swagger to enter your API key.

## 🧪 Running Tests

Ensure your code is reliable by running unit and integration tests regularly.

### 🔬 Unit Tests

Unit tests verify individual components in isolation.

1. **Navigate to the Tests Directory**

   ```bash
   cd src/Tests/ApplicationTests
   ```

2. **Run Unit Tests**

   ```bash
   dotnet test
   ```

   This command runs all unit tests and displays the results in the terminal.

### 🔍 Integration Tests

Integration tests verify the interactions between different parts of the application.

1. **Navigate to the Integration Tests Directory**

   ```bash
   cd src/Tests/Integration
   ```

2. **Run Integration Tests**

   ```bash
   dotnet test
   ```

   Integration tests will run and ensure that different components work together as expected.

## 📫 Contact

If you have any questions or need further assistance, feel free to reach out:

- **Email**: ezequielmora1986@gmail.com
- **GitHub**: [@ezequielmm](https://github.com/ezequielmm)
