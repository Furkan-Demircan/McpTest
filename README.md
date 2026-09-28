# 🎓 School & User Management System with AI Assistant (MCP)

[![Language](https://img.shields.io/badge/Language-English-blue.svg)](#) [![Turkish](https://img.shields.io/badge/Dokümantasyon-Türkçe-red.svg)](README_TR.md)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512bd4.svg?logo=dotnet)](https://dotnet.microsoft.com/)
[![React 19](https://img.shields.io/badge/React-19.0-61dafb.svg?logo=react)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0+-3178c6.svg?logo=typescript)](https://www.typescriptlang.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169e1.svg?logo=postgresql)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ed.svg?logo=docker)](https://www.docker.com/)
[![Model Context Protocol](https://img.shields.io/badge/MCP-Standard-purple.svg)](https://modelcontextprotocol.io/)

> 🇹🇷 **Türkçe dokümantasyon için [README_TR.md](README_TR.md) dosyasına göz atabilirsiniz.**

---

## 🌟 Overview

The **School & User Management System** is an enterprise-grade web application built around **Clean Architecture**, modern **React 19**, and the **Model Context Protocol (MCP)** standard.

The application enables seamless registration and management of **Students** and **Teachers**, validates critical Turkish identity credentials (TC Identity Number, unique email, date checks), and features an **autonomous AI Assistant** capable of voice dictation (Speech-to-Text), real-time form autofill via MCP tool-calling, and dynamic client-side page navigation.

```mermaid
flowchart LR
    subgraph Client ["Frontend (React 19 + TypeScript)"]
        UI["Web Interface & Forms"]
        STT["Web Speech API (Dictation)"]
        Widget["AI Assistant Widget"]
        ActionReg["Action Registry (Form Patch / Nav)"]
    end

    subgraph Backend [".NET 10 Clean Architecture API"]
        API["REST Controllers"]
        AI_Svc["AI Assistant Service"]
        User_Svc["User Service"]
        EF["Entity Framework Core"]
    end

    subgraph MCP_Server ["MCP Server (.NET 10)"]
        MCP_Tools["MCP Tools (FormTools, NavTools)"]
    end

    subgraph External ["External Services & DB"]
        DeepSeek["DeepSeek AI API"]
        PG[("PostgreSQL 16 DB")]
    end

    UI --> API
    Widget -->|Voice / Text Chat| AI_Svc
    STT --> Widget
    AI_Svc -->|Tool Calling Loop| DeepSeek
    AI_Svc -->|Call MCP Tools| MCP_Server
    MCP_Server -->|Structured Actions| AI_Svc
    AI_Svc -->|Form Patches & Navigation| ActionReg
    ActionReg --> UI
    API --> User_Svc
    User_Svc --> EF
    EF --> PG
```

---

## 🚀 Key Highlights

1. **Student Registration Page (`/form`, `/student`, `/ogrenci`)**:
   - First name, last name, 11-digit TC identity number, email, mother's name, father's name, birth date.
   - Instant client validation and server-side domain constraint checks.
   - Comprehensive registration receipt card upon submission.

2. **Teacher Registration Page (`/teacher`, `/ogretmen`)**:
   - Dedicated branch/specialty selection (*Mathematics, Physics, Chemistry, Biology, Turkish, IT/Software, English, etc.*) alongside standard identity fields.
   - Real-time suggestions datalist plus custom input support.
   - Independent state management and instant AI tool-calling support.

3. **Users Directory & Search (`/users`)**:
   - Live search filter across name, surname, TC number, email, and parents.
   - Real-time record statistics counter and manual refresh capabilities.

4. **Floating AI Assistant & Voice Dictation**:
   - **Speech-to-Text (STT)** powered by the browser Web Speech API (`tr-TR`), supporting continuous hands-free voice dictation.
   - Multi-turn conversation history with DeepSeek AI.
   - **MCP Tool-Calling Execution Loop**: As the user speaks or types (*e.g., "Set the student name to Ahmet and birth date to 2005-04-12"*), the backend invokes MCP tools and dispatches `fill_fields` actions; the client writes the values into the inputs on screen through the DOM, so the page's own `onChange` and validation run.
   - **Navigation Action**: The AI can programmatically redirect users (*e.g., "Take me to the teacher page"* triggers client-side navigation to `/teacher`).

5. **Model Context Protocol (MCP) Server**:
   - MCP endpoint hosted by the API with Streamable HTTP transport (`/mcp`).
   - Standardized tools for page identification, dynamic form schemas, form status checks, student form patching, and teacher form patching.

---

## 🏛️ Architecture & Technology Stack

### A. Frontend
- **Framework:** React 19, TypeScript, Vite
- **Routing:** `react-router-dom` v7 with aliases (`/form`, `/ogrenci`, `/teacher`, `/ogretmen`, `/users`)
- **State Management:** Pages keep their own local state (`useState`); the assistant never touches application state and works only through the DOM
- **Voice Recognition:** Web Speech API (`webkitSpeechRecognition` / `SpeechRecognition`) with auto-reconnection and continuous streaming
- **Styling:** Modern responsive CSS with custom design tokens, dark/light theme variables, glassmorphism, and responsive grid layouts

### B. Backend (.NET 10 Clean Architecture)
Organized into 4 decoupled layers adhering to Clean / Hexagonal Architecture:
- **`Domain`**: Pure business layer with zero third-party dependencies.
  - Entities: `User` entity containing encapsulation and domain validation.
  - Value Rules: 11-digit numeric TC validation, email structure, past date constraints.
  - Interfaces: `IUserRepository`.
  - Exceptions: `DomainException`.
- **`Application`**: Use cases, interfaces, and AI orchestration.
  - DTOs: `CreateUserDto`, `UserResponseDto`.
  - Services: `UserService`, `AiAssistantService`.
  - AI Contracts & Resolvers: `ToolContextResolver`, `IToolContextRegistry`.
- **`Infrastructure`**: External concerns and persistence.
  - PostgreSQL integration via Npgsql & Entity Framework Core.
  - Fluent API configurations with unique indexes on `TcNo` and `Email`.
  - `DeepSeekAiProvider`: DeepSeek chat completions client with function-calling support.
  - `McpClientService`: Integration with the MCP Server using `ModelContextProtocol.Client`.
- **`API`**: Presentation layer.
  - Controllers: `UsersController`, `AiAssistantController`, `McpController`.
  - Global Exception Handling Middleware (`GlobalExceptionMiddleware`).
  - CORS policy allowing full frontend communication.
  - Swagger / OpenAPI UI at root (`/`).

### C. MCP Server (`server/src/MCP`)
- Hosted by the API ASP.NET Core process, implementing the Model Context Protocol.
- Exposes tools via `ModelContextProtocol.Server` over HTTP transport.

### D. Database & Containers
- **Database:** PostgreSQL 16 Alpine
- **Containerization:** Multi-stage optimized Dockerfiles for API and MCP services, managed via `docker-compose.yml` with health checks.

---

## 📁 Directory Structure

```text
McpTest/
├── docker-compose.yml              # PostgreSQL and Backend API orchestrator (with hosted MCP endpoint)
├── .env.example                    # Template environment variables
├── README.md                       # English documentation (this file)
├── README_TR.md                    # Turkish documentation
├── PROMPT.md                       # Original architectural specifications
│
├── client/                         # Frontend Application (React 19 + TypeScript + Vite)
│   ├── src/
│   │   ├── app/
│   │   │   └── aiPages.ts          # Page catalog for the assistant (pages, paths, endpoints)
│   │   ├── assistant/              # Client-side assistant runtime (never touches app state)
│   │   │   ├── actions/            # fill_fields, highlight, navigation handlers
│   │   │   ├── dom/                # DOM adapter: findAiElement, writeValue
│   │   │   └── screenSnapshot.ts   # Live screen summary sent with each request
│   │   ├── components/
│   │   │   ├── AssistantWidget.tsx # Floating AI chat with Voice Dictation
│   │   │   └── AssistantWidget.css
│   │   ├── pages/                  # Regular hand-written React pages (local state)
│   │   │   ├── HomePage.tsx        # Welcome landing page
│   │   │   ├── FormPage.tsx        # Student registration form
│   │   │   ├── TeacherFormPage.tsx # Teacher registration form
│   │   │   └── UsersListPage.tsx   # Searchable data table
│   │   ├── services/
│   │   │   ├── api.ts              # REST client for /api/users
│   │   │   └── assistantApi.ts     # Client for /api/assistant/chat
│   │   ├── App.tsx                 # Route mapping
│   │   └── main.tsx                # Client entry point
│   ├── package.json
│   └── vite.config.ts
│
└── server/                         # .NET 10 Backend Solution
    ├── UserManagement.sln
    └── src/
        ├── Domain/                 # Entities, Domain Rules, Repository Interfaces
        ├── Application/            # DTOs, Service Interfaces, AI Assistant Pipeline
        ├── Infrastructure/         # EF Core, PostgreSQL, DeepSeek AI Client, MCP Client
        ├── API/                    # ASP.NET Core Web API Controllers & Middlewares
        └── MCP/                    # Model Context Protocol Server & Tools
```

---

## 🛠️ MCP Tools Reference

The MCP Server exposes the following specialized tools consumed by the DeepSeek AI Assistant:

| Tool Name | Parameters | Description | Output Target |
|---|---|---|---|
| `fill_fields` | `values` | Writes values into fields on the user's screen (form fields, search boxes, filters) through the DOM, so the page's own `onChange` runs. Keys are validated against the screen snapshot, or against the target page's Swagger fields after a navigation. | `type: "fill_fields"` |
| `navigate_to_page` | `page` (page id, path or alias) | Redirects the user to a catalog page; always emits the canonical path. | `type: "navigation"` |
| `highlight_element` | `elementId`, `message?` | Scrolls to and highlights an on-screen element (id, `name` or `data-ai-field`) with a short hint. Rejected if the element will not be visible to the user. | `type: "highlight"` |
| `search_app_knowledge` | `query` | Searches the process guides in `server/src/MCP/Knowledge/*.md`. | Guides |
| `get_page_schema` | `page` | Returns a page's catalog elements and, if it posts to an endpoint, the fields and rules from that endpoint's Swagger schema. | Schema object |
| `list_app_pages` | `query?`, `module?` | Searches pages by topic or module (max 10). Without arguments returns the module list (and pages if there are few). | Page list |
| `get_current_page` | `currentPage` (injected) | Returns the current page id, title and form. | Page object |

**The assistant knows the app without changing it:** the frontend stays a regular React app. The assistant reads the current screen from the DOM (live snapshot with labels and values), knows other pages from a small page catalog ([`client/src/app/aiPages.ts`](client/src/app/aiPages.ts): id, path, title, module and the endpoint the page posts to), and gets each form's fields and rules from that endpoint's **Swagger** schema (no backend annotations needed). Swagger is used **only as the schema/contract source**; usage guidance (how-tos, business rules, user-facing wording) lives in the separate Application Knowledge layer (`Knowledge/*.md`, `search_app_knowledge`). Screen fields map to Swagger fields by `data-ai-field` → `name` → `id`. `npm run pages` exports the catalog to the server, `npm run pages:check` validates it against the routes and JSX; the server validates endpoints and knowledge references at startup. See [FormEkleme.md](FormEkleme.md).

---

## 📡 REST API Endpoints

### User Management (`/api/users`)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/users` | Creates a new user/student/teacher record. Validates TC format, unique TC, unique email, and required parent/birthdate fields. |
| `GET` | `/api/users` | Retrieves all registered records ordered by creation date. |
| `GET` | `/api/users/{id}` | Retrieves a single user by GUID identifier. |
| `GET` | `/api/users/by-tc/{tcNo}` | Retrieves a single user by 11-digit TC number. |

### AI Assistant (`/api/assistant`)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/assistant/chat` | Accepts conversation history, current route, and active form data. Executes the DeepSeek function-calling loop with MCP tools, returning the assistant's response text and UI dispatch actions. |

---

## 🚀 Quickstart & Setup

### Prerequisites
- [Docker & Docker Desktop](https://www.docker.com/) (Recommended)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/) or [Bun](https://bun.sh/)
- A valid [DeepSeek API Key](https://platform.deepseek.com/)

---

### Option 1: Docker Compose (Recommended)

1. **Configure Environment Variables:**
   Create a `.env` file in the root directory (or copy from `.env.example`):
   ```bash
   cp .env.example .env
   ```
   Set your DeepSeek credentials:
   ```env
   POSTGRES_DB=usermanagement_db
   POSTGRES_USER=postgres
   POSTGRES_PASSWORD=postgrespassword
   DEEPSEEK_API_KEY=your_deepseek_api_key_here
   DEEPSEEK_BASE_URL=https://api.deepseek.com/
   DEEPSEEK_MODEL=deepseek-chat
   ```

2. **Start the Containers:**
   ```powershell
   docker compose up --build -d
   ```

   This launches:
   - **PostgreSQL 16:** `localhost:5432`
   - **Backend Web API:** `http://localhost:5000` (Swagger UI available at [http://localhost:5000](http://localhost:5000))
   - **MCP Server:** `http://localhost:5000/mcp` (hosted by the Backend Web API)

3. **Start the Frontend:**
   ```powershell
   cd client
   bun install   # or: npm install
   bun run dev   # or: npm run dev
   ```
   Open your browser at **`http://localhost:5173`**.

---

### Option 2: Local Development Without Docker

#### 1. Start PostgreSQL
Ensure PostgreSQL is running locally on port `5432` with database `usermanagement_db`.

#### 2. Start the Backend API and Hosted MCP Endpoint
```powershell
cd server/src/API
dotnet run --launch-profile API
```
*The API listens on `http://localhost:5000`; its MCP endpoint is available at `http://localhost:5000/mcp`.*

#### 3. Start Frontend
```powershell
cd client
bun install
bun run dev
```
*Available at `http://localhost:5173`.*

---

## 🎙️ Speech-to-Text (Voice Dictation)

The floating AI assistant in the lower-right corner includes a microphone button that leverages the **Web Speech API**:
- **Supported Browser:** Google Chrome, Microsoft Edge, and modern Chromium browsers.
- **Language:** Turkish (`tr-TR`).
- **Continuous Mode:** Keeps listening during pauses and converts interim chunks to final transcript.
- **Try it:** Click the microphone icon, say:
  > *"Adımı Mehmet, soyadımı Çelik, TC numaramı 12345678901 yap ve doğum tarihimi 15 Mayıs 1990 olarak ayarla"*
  
  The assistant interprets the sentence, invokes `fill_fields`, and instantly populates the input fields on the screen!

---

## 🧪 Verification & Build Checks

To verify that both frontend and backend compile without errors:

```powershell
# Build entire .NET solution
dotnet build server/UserManagement.sln

# Type-check and build React frontend
cd client
bun run build   # or: npm run build
```

---

## 📄 License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.