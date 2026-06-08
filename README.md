## Quick Start - FRONT END

### Prerequisites
- Node.js Latest version (v24.16.0)

### Setup

1. **Install dependencies:**
   ```bash
   npm install
   ```

2. **Create `.env.local` file (if not created yet):**
   ```
   VITE_FIREBASE_API_KEY=your_api_key
   VITE_FIREBASE_AUTH_DOMAIN=your_auth_domain
   VITE_FIREBASE_PROJECT_ID=your_project_id
   VITE_FIREBASE_STORAGE_BUCKET=your_storage_bucket
   VITE_FIREBASE_MESSAGING_SENDER_ID=your_messaging_sender_id
   VITE_FIREBASE_APP_ID=your_app_id
   ```

3. **Start development server:**
   ```bash
   npm run dev
   ```

   Open `http://localhost:5174` in your browser.

## Current Features

- **Authentication** - Signup/login with Firebase
- **Progressive Onboarding Flow** - Phone number collection
- **Dashboard** - View Current, Past, and Future tasks
- **Tasks** - Create, update, and delete tasks

## Tech Stack

- React 19.2.6
- Vite 8.0.12
- React Router 7.17.0
- Firebase 12.14.0
- Lucide React (icons)

## Available Commands

```bash
npm run dev      # Start dev server
npm run build    # Build for production
npm run preview  # Preview production build
npm lint         # Check code quality
```

## Pages & Routes

- `/` - Login
- `/signup` - Create account
- `/onboarding` - User profile setup
- `/dashboard` - Main dashboard

## Future Features

- **SSO** - Integrate SSO with the top 3 providers
- **Notifications warning** - When tasks are due to
- **Drag and Drop UI** - For better UX
- **Flexibility to change column names** - Allow the user to change names and titles for columns
- **Copy Card** - Ability to copy tasks
- **Polish UI** - Better layout UX

## What I Deliberately Left Out (And Why)
- UX polish items (toasts, shared loading system, a11y pass)
    - Deferred to keep scope on core functionality and API integration.

## What I Would Do With Another Day
1. Add better error UX (toasts + consistent empty/error/loading states).




## Quick Start - BACKEND


A secure task management API built with .NET 10 and Firebase Authentication. Users can create, manage, and organize their personal tasks with full data isolation.

## Features

### User Management
- User registration with Firebase Authentication
- Profile management (name, email, phone)
- Secure Firebase UID → Internal User ID mapping

### Task Management (CRUD)
- ✅ **Create** tasks with title, description, priority, category, and due date
- ✅ **Read** all your tasks or get a specific task by ID
- ✅ **Update** task details, status, priority, category, or due date
- ✅ **Delete** tasks you no longer need

### Task Features
- **Status Tracking**: Pending, InProgress, Completed, Cancelled
- **Priority Levels**: Low, Medium, High, Urgent
- **Categories**: Personal, Work, Shopping, Health, Finance, Education, Fitness, Other
- **Due Dates**: Required for all tasks (past or future dates accepted)
- **Timestamps**: Auto-tracked creation, update, and completion dates

### Security & Data Isolation
- 🔒 **Firebase JWT Authentication** - All endpoints except registration require valid Firebase tokens
- 🔒 **User Isolation** - Users can ONLY see and modify their own tasks
- 🔒 **No X-User-Id spoofing** - Authentication is cryptographically verified
- 🔒 **Tested Security** - 10 middleware tests + 21 service tests verify data isolation

## Requirements

- .NET 10 SDK
- SQLite (included, no setup needed)
- Firebase Project (for authentication)

## Setup Instructions

### 1. Clone and Restore Packages
```bash
git clone <repo-url>
cd FuncHealthDemo
dotnet restore
```

### 2. Firebase Configuration

**Option A: Environment Variable (Recommended for Production)**
```bash
export GOOGLE_APPLICATION_CREDENTIALS="/path/to/serviceAccountKey.json"
```

**Option B: File in Project Root (Development)**
- Download your Firebase service account key from Firebase Console
- Save as `serviceAccountKey.json` in the `FuncHealthDemo` project root
- The app will auto-detect and load it

**Option C: Skip Firebase for Local Testing**
- Firebase initialization will fail gracefully
- All endpoints will return 401/500 without valid tokens
- Use this only for local development testing

### 3. Run the Application
```bash
dotnet run --project FuncHealthDemo
```

The API runs on **http://localhost:5000**

Swagger docs available at **http://localhost:5000/swagger**


## API Endpoints

### Public Endpoints (No Auth Required)
```
POST /api/users - Register new user
```

## Database

**Engine:** SQLite (embedded, no server needed)

**File:** `health.db` (auto-created in project root)

**Migrations:** Auto-applied on startup via `EnsureCreated()`


### Short Term
- [ ] **Pagination** - Add `?page=1&pageSize=20` to GET /api/tasks
- [ ] **Filtering** - Filter tasks by status, priority, category
- [ ] **Sorting** - Sort by due date, priority, creation date
- [ ] **Search** - Search tasks by title/description

### Medium Term
- [ ] **Subtasks** - Break down tasks into smaller steps
- [ ] **Recurring Tasks** - Daily/weekly/monthly repeating tasks
- [ ] **Labels/Tags** - Flexible task organization beyond categories
- [ ] **Due Date Reminders** - Email/push notifications
- [ ] **Task Templates** - Pre-defined task structures

### Advanced Features
- [ ] **Shared Tasks** - Collaborate with other users
- [ ] **Team Workspaces** - Organizational task management
- [ ] **Task Assignment** - Assign tasks to team members
- [ ] **Comments & Attachments** - Rich task details
- [ ] **Activity Log** - Audit trail of changes
- [ ] **Analytics Dashboard** - Productivity insights

### Production Readiness
- [ ] **Rate Limiting** - Prevent API abuse
- [ ] **Logging** - Structured logs with Serilog
- [ ] **Health Checks** - `/health` endpoint for monitoring
- [ ] **HTTPS Enforcement** - Redirect HTTP to HTTPS
- [ ] **Docker Support** - Containerization
- [ ] **CI/CD Pipeline** - Automated testing and deployment
- [ ] **Environment Config** - dev/staging/prod settings
- [ ] **Secrets Management** - Azure Key Vault or similar
- [ ] **Soft Deletes** - Archive instead of hard delete
- [ ] **Database Migrations** - Versioned schema changes

### Testing Improvements
- [ ] **Integration Tests** - Test full API flows with real DB
- [ ] **Load Tests** - Performance under concurrent users
- [ ] **Security Audit** - Penetration testing
- [ ] **Code Coverage** - Aim for 80%+ coverage




