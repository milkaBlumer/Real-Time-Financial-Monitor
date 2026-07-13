#  Real-Time Financial Monitor

## 📖 Overview

**Real-Time Financial Monitor** is a full-stack application for monitoring financial transactions in real time.

The system includes:

- ⚙️ **ASP.NET Core 8 Web API** backend
- ⚛️ **React + TypeScript** frontend
- 📡 **SignalR** for real-time communication
- 🚀 **Redis** for caching and message distribution
- 🗄️ **SQLite** for persistent storage
- 🐳 **Docker** support for containerized deployment

---

# 📁 Project Structure

```text
Real-Time-Financial-Monitor
│
├── SQLINK-home-assignment
│   └── server-side2        # ASP.NET Core Backend
│
├── SQLINK-home-assignment-frontend
│   └── React + TypeScript Frontend
│
└── README.md
```

---

# 📋 Prerequisites

Before running the project, make sure the following tools are installed:

- Git
- .NET 8 SDK
- Node.js 20+
- npm
- Docker Desktop

---

# 📥 Clone the Repository

```bash
git clone https://github.com/milkaBlumer/Real-Time-Financial-Monitor.git
cd Real-Time-Financial-Monitor
```

---

# ▶️ Running the Project Locally

## 1️⃣ Start Redis

```bash
docker run --name sqlink-redis -p 6379:6379 -d redis:7-alpine
```

---

## 2️⃣ Run the Backend

Open a terminal:

```bash
cd SQLINK-home-assignment/server-side2

dotnet restore server-side2.sln

dotnet run --project server-side2/server-side2.csproj
```

---

## 3️⃣ Run the Frontend

Open another terminal:

```bash
cd SQLINK-home-assignment-frontend

npm install
```

Create a **.env** file inside the frontend project:

```env
VITE_API_BASE_URL=https://localhost:7276
VITE_WS_BASE_URL=wss://localhost:7276
```

Then run:

```bash
npm run dev
```

---

# 🌐 Default URLs

After the application starts:

| Service | URL |
|----------|-----|
| 🖥️ Frontend | http://localhost:5173 |
| ⚙️ Backend API | https://localhost:7276 |
| 📄 Swagger | https://localhost:7276/swagger |

---

# 🐳 Running with Docker

## 🔨 Build Backend Image

```bash
cd SQLINK-home-assignment/server-side2

docker build -t sqlink-backend:local .
```

---

## 🚀 Start Redis

```bash
docker run --name sqlink-redis -p 6379:6379 -d redis:7-alpine
```

---

## ▶️ Run Backend Container

```bash
docker run \
  --name sqlink-backend \
  -p 8080:8080 \
  -e Redis__ConnectionString=host.docker.internal:6379 \
  sqlink-backend:local
```

---

## ✅ Verify

Open:

```text
http://localhost:8080/health
```

---

# 🛠️ Technology Stack

### ⚙️ Backend

- ASP.NET Core 8
- Entity Framework Core
- SQLite
- Redis
- SignalR

### 🎨 Frontend

- React
- TypeScript
- Vite

### ☁️ Infrastructure

- Docker
- Redis
- REST API
- WebSockets (SignalR)

---

# ✨ Features

- 📥 Real-time transaction ingestion
- 📡 Live updates using SignalR
- 🗄️ Persistent transaction storage
- 🚀 Redis caching
- ❤️ Health endpoint
- 🐳 Docker support
- 🔒 Thread-safe concurrent processing
- 🌐 REST API
- 💻 Responsive React UI

---

# 🚀 Future Improvements

- ☸️ Kubernetes deployment
- 📈 Horizontal scaling
- 🔄 Redis Pub/Sub backplane
- 🔐 Authentication & Authorization
- ⚡ CI/CD pipeline
- 📊 Metrics & Monitoring
- ☁️ Cloud deployment (Azure / AWS)

---

# ❤️ Thank You

Thank you for taking the time to review this project.

I hope you enjoyed exploring the implementation as much as I enjoyed building it.

⭐ **Thanks for checking out this project!**

Have a wonderful day! 🚀
