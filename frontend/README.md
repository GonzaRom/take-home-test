# Loan Management Frontend

Angular frontend for the Loan Management MVP technical assessment.

The app consumes the ASP.NET Core REST API and provides a lightweight UI for
reviewing loans, creating new loans, viewing loan details, and registering
payments.

## Tech Stack

- Angular 19
- Angular Material
- TypeScript
- RxJS
- SCSS
- Angular HttpClient

## Features

- Paginated loan list using a Material table and paginator.
- Loan details dialog with payment history.
- Create loan dialog with reactive form validation.
- Register payment dialog with reactive form validation.
- Loading, empty, and error states for the main list.
- Success snackbars and backend validation error display.
- Basic responsive layout for the table and dialogs.

## Project Structure

The frontend uses standalone Angular components and keeps the loan feature under
`src/app/loans`.

- `containers/` holds page-level orchestration. The loan list container owns API
  calls, loading/error state, pagination, refresh triggers, and dialog flows.
- `presenters/` holds display-focused components. The loan table receives data
  through inputs and emits user actions through outputs.
- `dialogs/` holds Angular Material dialogs for creating loans, viewing loan
  details, and registering payments.
- `services/` contains the Angular HttpClient API service and small API error
  helpers.
- `models/` contains the TypeScript DTO shapes used by the UI.

## Running Locally

Install dependencies from the `frontend` directory:

```bash
npm install
```

Start the Angular development server:

```bash
npm start
```

Equivalent Angular CLI command:

```bash
ng serve
```

Open `http://localhost:4200/`.

The backend API must be running separately. In the default local setup the
frontend expects the API at `http://localhost:5080`.

## API Configuration

The API base URL is configured in:

```text
src/environments/environment.ts
```

Current value:

```ts
apiBaseUrl: 'http://localhost:5080'
```

## API Client Note

The project currently uses Angular HttpClient with manually defined TypeScript
models for the backend contracts.

A future improvement would be generating TypeScript models and an API client
from the backend OpenAPI contract using a tool such as
`swagger-typescript-api`.

## Tradeoffs

- No NgRx is used because the current state needs are simple and local to the
  loan page.
- RxJS is used pragmatically for API loading, refresh triggers, pagination, and
  dialog submit flows.
- The frontend is intentionally lightweight because this assessment is focused
  on backend and fullstack integration.
- Authentication is not implemented in this MVP.

## Future Improvements

- Generate the API client and DTOs from OpenAPI.
- Improve form validation UX and field-level backend error mapping.
- Add authentication and route guards.
- Add E2E tests for the main loan workflows.
- Continue accessibility review for dialogs, table actions, and form states.
