## The Metric Convert

The Metric Convert is a small learning app that helps people become comfortable with the metric system by
practising conversions and seeing how they work step-by-step.

This repo is split into:

- **backend**: ASP.NET Core Web API (`TheMetricConvert.Api`)
- **frontend**: Angular app (standalone components, Angular v21)

---

### Backend (C# API)

- Project: `backend/TheMetricConvert.Api`
- Framework: .NET 10 (`net10.0`)

**Prerequisites:** Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (your machine may only have older SDKs until you add it). Verify with `dotnet --list-sdks`.

Key endpoints:

- `GET /healthz` – basic liveness probe.
- `GET /api/units` – returns the unit catalog used by the converter (symbols, names, categories, metric prefix metadata).
- `POST /api/conversions` – converts `{ from, to, value }` and returns:
  - `outputValue` + `outputUnit`
  - `steps`: human-readable explanation of the math
  - `tip`: extra learning hint, especially for metric prefixes

Swagger:

- URL: `http://localhost:5080/swagger`
- Uses Swashbuckle, with XML-doc comments and a small custom UI skin.

Run locally:

```bash
cd backend/TheMetricConvert.Api
dotnet run --urls http://localhost:5080
```

---

### Frontend (Angular)

- Location: `frontend/`
- Tooling: Angular CLI v21, standalone components, SCSS

Key pieces:

- `App` shell (`app.ts`, `app.html`, `app.scss`)
  - Top header with app name + short tagline.
  - Navigation:
    - `Converter` → `/`
    - `Learn` → `/learn`
    - `API docs` → opens backend Swagger in a new tab.

- `ApiService` (`api.service.ts`)
  - Wraps HTTP calls to the backend:
    - `getUnits()` → `GET /api/units`
    - `convert(body)` → `POST /api/conversions`
  - For now the base URL is hard-coded to `http://localhost:5080`
    (look for the `TODO` if you want to make this environment-based).

- `ConverterComponent` (`converter.component.*`, route `/`)
  - Form with:
    - numeric `value`
    - `from` unit dropdown
    - `to` unit dropdown (both populated from `/api/units`)
  - Calls the API on submit and renders:
    - conversion result
    - step list
    - optional learning tip

- `LearnComponent` (`learn.component.*`, route `/learn`)
  - Static content explaining:
    - base units (m, g, L)
    - common metric prefixes (milli, centi, kilo)
    - how metric vs imperial thinking differs
  - Contains `TODO`s where you can later break this into richer lessons/quizzes.

Run the frontend:

```bash
cd frontend
npm install        # first time only
npm start          # or: npx ng serve
# App will be on http://localhost:4200
```

Make sure the backend is running on `http://localhost:5080` before trying conversions, or the UI
will show a friendly error message.

---

### Development notes

- The backend is commented with intent-focused XML doc comments so Swagger reads well.
- The Angular app includes inline comments and `TODO`s in strategic places (API base URL, lesson
  structure, potential future view-model extraction) to guide future enhancements without
  cluttering the templates.

