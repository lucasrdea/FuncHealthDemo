# FuncHealth Frontend

This is the patient-facing frontend for the FuncHealth demo.

It covers the main user journey end to end:
- create account / login
- complete onboarding
- book and manage appointments
- view past appointment results and download the report PDF

## How To Run
1. Create `.env.local` with Firebase values:
	 - `VITE_FIREBASE_API_KEY`
	 - `VITE_FIREBASE_AUTH_DOMAIN`
	 - `VITE_FIREBASE_PROJECT_ID`
	 - `VITE_FIREBASE_STORAGE_BUCKET`
	 - `VITE_FIREBASE_MESSAGING_SENDER_ID`
	 - `VITE_FIREBASE_APP_ID`
2. Install dependencies:
	 - `npm install`
3. Start development server:
	 - `npm run dev`

## What I Built
- Firebase authentication (signup/login)
- Onboarding form (phone number + date of birth)
- Dashboard with:
	- upcoming appointments
	- past appointments
- Appointment actions:
	- create
	- update
	- delete
- Results flow:
	- open appointment result details
	- download PDF report
- Auth handling in API layer:
	- sends Bearer token on requests
	- retries once on 401 after token refresh

## Routes
- `/` login
- `/signup`
- `/onboarding`
- `/dashboard`
- `/booking`
- `/appointment/changes`
- `/appointment/results/:id`

## API Endpoints Used
- `POST /users`
- `GET /users/:id`
- `PUT /users/profile`
- `GET /labexams`
- `GET /appointments/user/:userId`
- `POST /appointments`
- `PUT /appointments/:id`
- `DELETE /appointments/:id`
- `GET /appointments/results/:id`
- `GET /appointments/results/report`

## What I Deliberately Left Out (And Why)
- `GET /users/me` migration
	- Current code still uses `GET /users/:id` because backend/user-id mapping was already wired this way during implementation.
- Automated tests
	- Focus was on delivering the full feature flow first.
- UX polish items (toasts, shared loading system, a11y pass)
	- Deferred to keep scope on core functionality and API integration.

## What I Would Do With Another Day
1. Replace `GET /users/:id` with `GET /users/me` and remove frontend user-id mapping complexity.
2. Add tests for auth, booking, and 401 refresh behavior.
3. Add better error UX (toasts + consistent empty/error/loading states).
4. Do an accessibility pass (keyboard flow, ARIA checks, contrast fixes).
