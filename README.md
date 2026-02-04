# Quiz ThaiBev - IT Quiz Application

A full-stack quiz application built for ThaiBev IT events. This project features a Vue.js frontend for user interaction (registration, quizzes, barcode generation) and a .NET 8 backend API for data management.

## 🔗 Live Demo
- **Production URL**: [https://quiz-thaibev-front.onrender.com/](https://quiz-thaibev-front.onrender.com/)


## 🚀 Tech Stack

### Frontend
- **Framework**: Vue.js 3
- **Build Tool**: Vite
- **Routing**: Vue Router
- **Utilities**: JSBarcode, QRCode

### Backend
- **Framework**: ASP.NET Core (.NET 8)
- **Database**: SQLite (local)
- **Documentation**: Swagger UI
- **Security**: JWT Authentication

### Deployment
- **Containerization**: Docker
- **Cloud Platform**: Render

---

## 🛠️ Prerequisites

Before you begin, ensure you have met the following requirements:
- **Node.js** (v18+ recommended)
- **.NET SDK** 8.0
- **Docker** (optional, for containerized run)

---

## 💻 Local Setup & Running

### 1. Backend API

```bash
# Navigate to backend directory
cd backend

# Restore dependencies
dotnet restore

# Run the application
dotnet run --project Quiz.Api
```
The API will start at `http://localhost:5173` (or port configured in launchSettings).
Swagger UI available at: `/swagger` (Development mode only).

### 2. Frontend

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Run development server
npm run dev
```
The frontend will start at `http://localhost:5173` (default Vite port).

---

## 🐳 Docker Deployment

To build and run the backend container locally:

```bash
# Build the image
docker build -t quiz-backend -f backend/Dockerfile .

# Run the container
docker run -p 8080:8080 quiz-backend
```

---

## ☁️ Deployment on Render

This project is configured for deployment on **Render**.

1.  **Connect GitHub**: Create a new Web Service on Render and connect this repository.
2.  **Configuration**: Render will automatically detect `render.yaml`.
3.  **Environment Variables**:
    You **MUST** set the following environment variables in the Render Dashboard:

    | Variable | Description | Required? |
    | :--- | :--- | :--- |
    | `JWT_SECRET_KEY` | A random string (min 32 chars) for token signing. | **YES** |
    | `CORS_ALLOWED_ORIGINS` | URL of your frontend (e.g., `https://myapp.onrender.com`). | Recommended |

    _Note: The application will fail to start in Production if `JWT_SECRET_KEY` is missing._

---

## 📂 Project Structure

- `backend/` - ASP.NET Core Web API solution.
- `frontend/` - Vue.js + Vite source code.
- `render.yaml` - Render infrastructure configuration.
