---
title: "BatchJobs + PACT API - Complete Documentation Index"
version: "1.0"
audience: "All Stakeholders"
---

# 📚 Complete Documentation Index

## 🌟 START HERE (Everyone)

**[MASTER-IMPLEMENTATION-GUIDE.md](MASTER-IMPLEMENTATION-GUIDE.md)** ← **READ THIS FIRST**

Consolidated single document with everything:
- Architecture overview
- Full 7-state machine contract
- Complete API specification
- Watchdog algorithm
- Design rationale & architect Q&A
- Deployment guidance
- Team responsibilities
- Implementation checklist
- Sign-off section

**Read this first, then use detailed references below for your specific role.**

---

## For Different Audiences

### 🎯 PACT API Development Team
**Start Here**: [MASTER-IMPLEMENTATION-GUIDE.md](MASTER-IMPLEMENTATION-GUIDE.md) - Sections 1-4 (Architecture, Contract, API, Watchdog)

Then read in order:
1. [PACT-API-QUICK-REFERENCE.md](PACT-API-QUICK-REFERENCE.md) - Developer reference (5 min, bookmark this!)
2. [PACT-API-OPENAPI.yaml](PACT-API-OPENAPI.yaml) - Machine-readable API spec (import into IDE)
3. [PACT-API-HANDOFF.md](PACT-API-HANDOFF.md) - Implementation roadmap
4. [DEMO-END-TO-END-FLOW.md](DEMO-END-TO-END-FLOW.md) - Test against these scenarios

### 🧪 QA / Testing Team
**Start Here**: [MASTER-IMPLEMENTATION-GUIDE.md](MASTER-IMPLEMENTATION-GUIDE.md) - Sections 1-2 (Architecture, State Machine)

Then read:
1. [DEMO-END-TO-END-FLOW.md](DEMO-END-TO-END-FLOW.md) - Your testing scenarios
2. [PACT-API-QUICK-REFERENCE.md](PACT-API-QUICK-REFERENCE.md) - Error codes section
3. [BATCHJOBS-STATE-MACHINE-CONTRACT.md](BATCHJOBS-STATE-MACHINE-CONTRACT.md) - Section 8 (Testing Scenarios)

### 👔 Project Managers / Stakeholders
**Start Here**: [MASTER-IMPLEMENTATION-GUIDE.md](MASTER-IMPLEMENTATION-GUIDE.md) - Executive Summary + Section 1 (Architecture)

Then read:
- Section 2 (State Machine Overview)
- [DEMO-END-TO-END-FLOW.md](DEMO-END-TO-END-FLOW.md) - See it in action

### 🏗️ System Architects
**Start Here**: [MASTER-IMPLEMENTATION-GUIDE.md](MASTER-IMPLEMENTATION-GUIDE.md) - Sections 1-2 (Architecture), Section 5 (Design Rationale & Q&A)

Then read:
1. [PACT-BATCHJOBS-HANDOFF-EVENTGRID.md](PACT-BATCHJOBS-HANDOFF-EVENTGRID.md) - Full technical architecture
2. [EVENTBRIDGE-ALIGNMENT-GUIDE.md](EVENTBRIDGE-ALIGNMENT-GUIDE.md) - Production EventBridge design
3. [PACT-API-OPENAPI.yaml](PACT-API-OPENAPI.yaml) - API design
4. [BATCHJOBS-STATE-MACHINE-CONTRACT.md](BATCHJOBS-STATE-MACHINE-CONTRACT.md) - Formal contract

### 🚀 DevOps / Deployment Engineers
**Start Here**: [MASTER-IMPLEMENTATION-GUIDE.md](MASTER-IMPLEMENTATION-GUIDE.md) - Section 6 (Deployment Guide)

Then read:
1. [EVENTBRIDGE-ALIGNMENT-GUIDE.md](EVENTBRIDGE-ALIGNMENT-GUIDE.md) - EventBridge setup
2. [MASTER-IMPLEMENTATION-GUIDE.md](MASTER-IMPLEMENTATION-GUIDE.md) - Section 8 (Implementation Checklist)

### 🎓 New Team Members (Onboarding)
1. Read this index (you are here!)
2. Watch the demo (Sample UI at localhost:5003)
3. Read [BATCHJOBS-STATE-MACHINE-CONTRACT.md](BATCHJOBS-STATE-MACHINE-CONTRACT.md) - Sections 1-2
4. Bookmark [PACT-API-QUICK-REFERENCE.md](PACT-API-QUICK-REFERENCE.md)
5. Explore [PACT-API-OPENAPI.yaml](PACT-API-OPENAPI.yaml) in OpenAPI viewer
6. Ask questions on Slack: #batch-jobs-integration

---

## Document Descriptions

### 📋 PACT-API-HANDOFF.md
**What**: Official handoff document from BatchJobs to PACT API team  
**Why**: Confirms readiness for implementation, provides checklist, links all resources  
**Read Time**: 15-20 minutes  
**Key Sections**:
- What PACT API Must Implement (2 endpoints)
- The 7 Job States (diagram)
- Watchdog Algorithm (pseudo-code)
- Implementation Checklist (Phase 1-4)
- Success Criteria (go/no-go gates)

### 🔐 BATCHJOBS-STATE-MACHINE-CONTRACT.md
**What**: Formal specification document (legal contract between teams)  
**Why**: Defines exact behavior, error codes, boundaries, sign-off requirements  
**Read Time**: 30-40 minutes  
**Key Sections**:
1. State Definitions (7 persistent + 3 transient)
2. State Transition Rules (FSM diagram + invalid transitions)
3. API Contract (request/response schemas)
4. Trigger Endpoint (202/409/400 responses)
5. Watchdog Algorithm (SQL-like pseudo-code)
6. Responsibilities Matrix (who owns what)
7. Testing Scenarios (validation checklist)
8. Deployment Checklist (pre-production steps)
9. Sign-Off (formal commitment)

**Critical**: This is the source-of-truth contract. **NOTHING supersedes this document.**

### 🚀 PACT-API-OPENAPI.yaml
**What**: Machine-readable OpenAPI 3.0 specification  
**Why**: Can be imported into Postman, Swagger UI, IDE code generators  
**Read Time**: 15 minutes (skim paths, read schemas)  
**Key Sections**:
- /api/v1/batch-jobs/trigger (POST) with request/response schemas
- /api/batch-jobs/{jobName}/status (GET) with polling recommendations
- All error responses (400, 404, 409, 500)
- Full schema definitions for all request/response objects

**How to Use**:
1. Import into Postman: File → Import → Paste YAML
2. View in editor: https://editor.swagger.io → Paste YAML
3. Generate client: OpenAPI Generator → select language → generate code

### 📖 PACT-API-QUICK-REFERENCE.md
**What**: One-page developer cheat sheet  
**Why**: Quick lookup during coding; bookmark this!  
**Read Time**: 5-10 minutes (reference)  
**Key Sections**:
- 7 Persistent DB States (one-liner descriptions)
- 3 Transient Watchdog States (when each appears)
- Trigger Endpoint Quickstart (curl example)
- Status Endpoint Quickstart (curl example + response)
- Watchdog Algorithm (pseudo-code)
- Polling Strategy (decision tree)
- Client Decision Logic (JavaScript example)
- Common Patterns (4 reusable snippets)
- Testing Checklist (before go-live)

**How to Use**: Print this, tape it to monitor, refer constantly during coding.

### 🎬 DEMO-END-TO-END-FLOW.md
**What**: Complete demo walkthrough with exact timelines  
**Why**: Validates implementation; shows realistic flows; testing guide  
**Read Time**: 20-30 minutes  
**Key Sections**:
- Demo Setup Verification Checklist (pre-demo steps)
- Scenario 1: Happy Path (T+0s to T+50s timeline)
- Scenario 2: Watchdog Timeout (T+0s to T+190s timeline)
- Scenario 3: Retry Flow (full lifecycle with retry)
- Scenario 4: Concurrent Rejection (409 conflict)
- Formal Contract Validation Checklist (validation gates)
- Demo Script (presenter talking points)
- Troubleshooting During Demo (common issues + fixes)

**How to Use**: 
1. Run demo against Sample UI (localhost:5003)
2. Refer to timeline for expected state at each T+Xs
3. Use validation checklist to confirm behavior
4. Use troubleshooting section if stuck

### 🏛️ PACT-BATCHJOBS-HANDOFF-EVENTGRID.md
**What**: Comprehensive architecture and handoff document  
**Why**: Deep-dive into watchdog mechanism, state transitions, architectural patterns  
**Read Time**: 25-30 minutes (thorough)  
**Key Sections**:
- Runtime Architecture (current + target)
- Shared Transition Model (state definitions + DB schema)
- Watchdog Mechanism (algorithm, examples, edge cases)
- API Contract Details (status endpoint semantics)
- Polling Client Recommendations (strategy for consumers)
- Database Schema (Alibaba-style reference)
- Deprecated vs. Active States (clarification)
- Troubleshooting Guide (common issues)

**How to Use**: Reference for architectural questions; deep understanding of watchdog.

---

## Quick Navigation by Task

### "I need to implement the trigger endpoint"
→ [PACT-API-OPENAPI.yaml](PACT-API-OPENAPI.yaml) (paths → /api/v1/batch-jobs/trigger)  
→ [PACT-API-HANDOFF.md](PACT-API-HANDOFF.md) (What PACT API Must Implement)

### "I need to implement the status endpoint"
→ [PACT-API-OPENAPI.yaml](PACT-API-OPENAPI.yaml) (paths → /api/batch-jobs/{jobName}/status)  
→ [BATCHJOBS-STATE-MACHINE-CONTRACT.md](BATCHJOBS-STATE-MACHINE-CONTRACT.md) (Section 3)  
→ [PACT-BATCHJOBS-HANDOFF-EVENTGRID.md](PACT-BATCHJOBS-HANDOFF-EVENTGRID.md) (Section 7.2)

### "I need to understand the watchdog algorithm"
→ [PACT-API-HANDOFF.md](PACT-API-HANDOFF.md) (Watchdog Algorithm - Critical)  
→ [PACT-API-QUICK-REFERENCE.md](PACT-API-QUICK-REFERENCE.md) (Watchdog Algorithm section)  
→ [PACT-BATCHJOBS-HANDOFF-EVENTGRID.md](PACT-BATCHJOBS-HANDOFF-EVENTGRID.md) (Section 4.3)

### "I need to test my implementation"
→ [DEMO-END-TO-END-FLOW.md](DEMO-END-TO-END-FLOW.md) (All 4 scenarios)  
→ [BATCHJOBS-STATE-MACHINE-CONTRACT.md](BATCHJOBS-STATE-MACHINE-CONTRACT.md) (Section 8 - Testing Scenarios)  
→ [PACT-API-QUICK-REFERENCE.md](PACT-API-QUICK-REFERENCE.md) (Testing Checklist)

### "I need to understand the 7 states"
→ [PACT-API-QUICK-REFERENCE.md](PACT-API-QUICK-REFERENCE.md) (7 Persistent DB States)  
→ [BATCHJOBS-STATE-MACHINE-CONTRACT.md](BATCHJOBS-STATE-MACHINE-CONTRACT.md) (Section 1)  
→ [PACT-BATCHJOBS-HANDOFF-EVENTGRID.md](PACT-BATCHJOBS-HANDOFF-EVENTGRID.md) (Section 4.1)

### "I need to know error codes"
→ [PACT-API-QUICK-REFERENCE.md](PACT-API-QUICK-REFERENCE.md) (Error Codes section)  
→ [BATCHJOBS-STATE-MACHINE-CONTRACT.md](BATCHJOBS-STATE-MACHINE-CONTRACT.md) (Section 4.3)  
→ [PACT-API-OPENAPI.yaml](PACT-API-OPENAPI.yaml) (responses section)

### "I need to present this to my team"
→ [PACT-API-HANDOFF.md](PACT-API-HANDOFF.md) (Executive Summary + Implementation Checklist)  
→ [DEMO-END-TO-END-FLOW.md](DEMO-END-TO-END-FLOW.md) (Demo Script section)  
→ Sample UI demo at localhost:5003

---

## Implementation Timeline Recommendation

| Week | Milestone | Documents to Read | Deliverables |
|------|-----------|-------------------|--------------|
| **1** | **Setup & Planning** | PACT-API-HANDOFF, BATCHJOBS-STATE-MACHINE-CONTRACT (Sections 1-2) | Phase 1: Endpoints implemented |
| **1-2** | **Watchdog Logic** | PACT-API-HANDOFF (Watchdog section), PACT-API-QUICK-REFERENCE | Phase 2: Watchdog tested |
| **2** | **Testing & Validation** | DEMO-END-TO-END-FLOW, PACT-API-QUICK-REFERENCE (Testing Checklist) | Phase 3: 4 scenarios pass |
| **2-3** | **Documentation & Sign-Off** | BATCHJOBS-STATE-MACHINE-CONTRACT (Section 11) | Phase 4: Contract signed |

---

## Key Takeaways (TL;DR)

1. **7 states** in database: Pending, Running, Completed, Failed, Cancelled, Retry, Skipped
2. **2 endpoints** to build: POST /trigger (202) and GET /status (200)
3. **Watchdog logic**: Compute timeout if no DB record after acceptedAtUtc + SLA deadline
4. **HTTP 409**: Return conflict if second trigger sent during Running state
5. **Polling strategy**: 2-5s startup, 15-30s running, stop at terminal
6. **Source of truth**: Database is authoritative; PACT provides transient projections
7. **Contract**: Read, understand, implement exactly; sign off when complete

---

## Questions & Support

| Type | Answer Location | Contact |
|------|-----------------|---------|
| API field meanings | PACT-API-OPENAPI.yaml (field descriptions) | PACT API Tech Lead |
| State machine behavior | BATCHJOBS-STATE-MACHINE-CONTRACT.md (Section 2) | BatchJobs Lead |
| Watchdog algorithm | PACT-API-HANDOFF.md or PACT-API-QUICK-REFERENCE.md | Both team leads |
| Test scenario failures | DEMO-END-TO-END-FLOW.md (Troubleshooting) | QA Lead |
| Urgent bugs | Escalation to both leads | Product Manager |

---

## Acknowledgments

- **BatchJobs Team**: Specification authorship, state machine design, database schema
- **PACT API Team**: Implementation feedback, architectural guidance
- **Architecture Review**: Contract validation, best practices review
- **QA Team**: Demo scenario validation, testing guidelines

---

**Documentation Version**: 1.0  
**Last Updated**: 2026-06-03  
**Status**: Production-Ready  
**Questions?** See Support section above.
