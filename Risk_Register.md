# CivicConnect Risk Register

This document tracks project-specific risks and their downstream consequences. Risks are prioritized to ensure the engineering baseline remains controlled despite schedule, resource, or scope constraints.

| Risk Code & Title | Prob. | Impact | Prio. | Mitigation Strategy | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **R-01: Scope Creep** | High | High | High | Stuck strictly to MoSCoW scope baseline. | Active |
| **R-02: Undersized Team Constraint** | High | Medium | Medium | Lecturer exemption granted; modular decoupling applied. | Mitigated (Exemption Granted) |
| **R-03: Security & Privacy Compliance** | Medium | High | Medium | Centralized RBAC middleware and cookie auth enforced. | Active |
| **R-04: R0 Budget Limit** | Medium | High | High | Designed schema within free-tier row quotas. | Active |
| **R-05: Database Query Bottlenecks (M2)** | Medium | High | High | Applied non-clustered indexing strategy on key columns. | Active |
| **R-06: Domain & Controller Coupling (M2)** | Medium | Medium | Medium | Isolated category validation via Strategy Pattern (ADR-004). | Active |
| **R-07: Environment Drift (M2)** | Low | High | Low | Locked C# .NET SDK versions in config files. | Active |
| **R-08: Staging & Persistence Boundary (M3)** | Medium | Medium | Medium | Adopted thread-safe in-memory singleton for presentation stability. | Active |demonstration.

Owner: Bernu

Status: Active
