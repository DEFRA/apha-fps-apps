# Backend Analysis — FPS MaintDG

## Reference Map

| Referenced name | Type | Triggering event / context | Parameters / notes |
|---|---|---|---|
| `SELECT [GradeCode] FROM [Grade]` | Inline RowSource SQL | `GradeCode` control RowSource | LINQ: `_context.Grades.Select(g => g.GradeCode)` |
| `SELECT [DivName] FROM [tlkpDivision]` | Inline RowSource SQL | `Division` control RowSource | Existing `IDivisionRepository.GetAllDivisionsAsync()` |

## Artefact Detail

No named queries, stored procedures, views, triggers, or BAS functions referenced.

- **Optimization notes:** Both RowSource selects are simple single-column lookups; mapped to direct EF queries returning `List<string>`.

## C# Artefact Mapping

| VBA RowSource | Repository method | Notes |
|---|---|---|
| `SELECT [GradeCode] FROM [Grade]` | `IDivisionGradeRepository.GetAllGradeCodesAsync()` | Uses `_context.Grades.AsNoTracking().Select(g => g.GradeCode)` |
| `SELECT [DivName] FROM [tlkpDivision]` | `IDivisionRepository.GetAllDivisionsAsync()` | Already exists |

## Raw SQL Decisions

No raw SQL used. All operations implemented with EF Core LINQ.

## File Changes — Phase 1 Backend

| # | Action | File path (relative to `src/`) | Reason |
|---|--------|-------------------------------|--------|
| 1 | CREATE | `Apha.Common/Contracts/FPS/DivisionGradeReq.cs` | API request contract |
| 2 | CREATE | `Apha.Common/Contracts/FPS/DivisionGradeRes.cs` | API response contract |
| 3 | CREATE | `Apha.FPS/Apha.FPS.Core/Entities/Grade.cs` | Grade entity for dropdown lookup |
| 4 | CREATE | `Apha.FPS/Apha.FPS.Core/Interfaces/IDivisionGradeRepository.cs` | Repository interface |
| 5 | CREATE | `Apha.FPS/Apha.FPS.Application/Dtos/DivisionGradeDto.cs` | Application DTO |
| 6 | CREATE | `Apha.FPS/Apha.FPS.Application/Interfaces/IMaintDGService.cs` | Service interface |
| 7 | CREATE | `Apha.FPS/Apha.FPS.Application/Services/MaintDGService.cs` | Service implementation |
| 8 | CREATE | `Apha.FPS/Apha.FPS.DataAccess/Data/GradeMap.cs` | Grade EF configuration |
| 9 | MODIFY | `Apha.FPS/Apha.FPS.DataAccess/Data/FpsDbContext.cs` | Add `DbSet<Grade>` + `GradeMap` |
| 10 | CREATE | `Apha.FPS/Apha.FPS.DataAccess/Repositories/DivisionGradeRepository.cs` | Repository implementation |
| 11 | CREATE | `Apha.FPS/Apha.FPS.Api/Controllers/MaintDGController.cs` | API controller |
| 12 | MODIFY | `Apha.FPS/Apha.FPS.Application/Mappings/EntityMapper.cs` | Add DivisionGrade + Grade mappings |
| 13 | MODIFY | `Apha.FPS/Apha.FPS.Api/Mappings/RequestMapper.cs` | Add DivisionGrade mappings |
| 14 | MODIFY | `Apha.FPS/Apha.FPS.Api/Extensions/ServiceCollectionExtension.cs` | Register service + repository |
| 15 | MODIFY | `Apha.Common/Constants/FpsApiEndpoints.cs` | Add DivisionGrade endpoint constants |
