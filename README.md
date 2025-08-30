# 🛒 TiendaWed  

**Autor:** Jose Hernandez  
**Fecha:** 2025  

TiendaWed es una aplicación web desarrollada en **ASP.NET Core MVC** con **SQL Server**, orientada a la **gestión de inventario, ventas, compras y usuarios**.  
El sistema está diseñado para pequeñas y medianas empresas que buscan optimizar sus procesos de control de stock y ventas.  

---

## 📌 Funcionalidades principales  
- 🔑 Autenticación de usuarios (clientes y administradores).  
- 🛍️ Carrito de compras.  
- 📦 Gestión de inventario (CRUD de productos).  
- 🧾 Registro de compras y ventas.  
- 📑 Reportes en PDF.  
- 👥 Gestión de proveedores y clientes.  
- ☁️ Despliegue en la nube (Azure / Docker).  

---

## 🏗️ Arquitectura técnica  
- **Backend:** ASP.NET Core 8 (MVC).  
- **Base de datos:** SQL Server.  
- **Frontend:** Razor Pages + Bootstrap.  
- **Despliegue:** Visual Studio 2022 / Docker / Azure App Service.  

---

## ⚙️ Requisitos previos  
Antes de instalar el proyecto asegúrate de tener:  
- .NET 8 SDK  
- SQL Server (local o en la nube)  
- Visual Studio 2022 o VS Code  
- Docker (opcional, para contenedores)  

---

## 🚀 Instalación en local  

### 1️⃣ Clonar el repositorio  
```bash
git clone https://github.com/jfh23tt/TiendaWed.git
cd TiendaWed
2️⃣ Restaurar dependencias
dotnet restore

3️⃣ Configurar la cadena de conexión

Editar el archivo appsettings.json con tus credenciales de SQL Server:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:netsde.database.windows.net,1433;Initial Catalog=Datos;Persist Security Info=False;User ID=User;Password=pasword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}


4️⃣ Aplicar migraciones a la base de datos
dotnet ef database update

5️⃣ Ejecutar el proyecto
dotnet run
