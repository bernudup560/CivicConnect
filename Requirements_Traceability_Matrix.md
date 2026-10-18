# Prioritized Requirements Baseline & Acceptance Criteria

This document details the functional and non-functional requirements baseline for CivicConnect, outlining specific priorities, sources, descriptions, and verifiable acceptance criteria.

---

## Functional Requirements

### FR-001: Categorized Request Intake
* **Priority:** High
* **Source:** Requester Group
* **Description:** The system shall allow logged-in Requesters to create a new service request by selecting a predefined category and supplying a detailed text description.
* **Acceptance Criteria:**
  * **Input Validation:** Category selection from the dropdown is mandatory.
  * **Text Boundary:** Description field must contain at least 15 characters; submissions below this threshold are rejected with an inline validation error.
  * **Persistence & ID:** Successful submission creates a persistent record, generates a unique non-sequential tracking ID, sets the initial status to `Open`, and logs the creation timestamp.

---

### FR-002: Workflow Status Transitions
* **Priority:** High
* **Source:** Field Staff Group
* **Description:** Authorized Staff users shall be able to transition a request's lifecycle state from `Open` to `In Progress` or `Resolved`.
* **Acceptance Criteria:**
  * **RBAC Controls:** Status modification controls are visible and actionable only to authenticated Staff and Management users.
  * **Auditability:** Every state change appends an immutable audit log record capturing `UserID`, `PreviousStatus`, `NewStatus`, and `Timestamp`.
  * **Resolution Requirement:** Transitioning to `Resolved` strictly requires a resolution summary note (minimum 10 characters).

---

### FR-003: Overdue Task Analytics
* **Priority:** Medium
* **Source:** Executive Management
* **Description:** The system highlights active requests exceeding 48-hour operational SLA timelines in management reporting views.
* **Acceptance Criteria:**
  * **SLA Logic:** System automatically calculates elapsed time ($\text{elapsed time} = \text{current time} - \text{created time}$) for all non-resolved requests.
  * **Overdue Flag:** Any active ticket with $\text{elapsed time} > 48\text{ hours}$ is dynamically flagged as `Overdue` in summary dashboards and export reports.

---

## Non-Functional Requirements

### NFR-001: Query Performance & Response Latency
* **Priority:** High (Quality Guardrail)
* **Description:** The backend service must maintain responsive data retrieval operations under standard operational loads.
* **Acceptance Criteria:**
  * Response time for query/retrieval endpoints must be $\le 2.0\text{ seconds}$ at the 95th percentile under a simulated load of 50 concurrent virtual users.

---

### NFR-002: Role-Based Authorization Enforcement
* **Priority:** High (System Security)
* **Description:** Server-side authorization middleware must enforce strict multi-tenant role separation (`Requester`, `Staff`, `Management`).
* **Acceptance Criteria:**
  * All restricted API endpoints validate claims server-side.
  * Requests without valid authentication tokens return `HTTP 401 Unauthorized`.
  * Authenticated requests lacking necessary role permissions return `HTTP 403 Forbidden`.
