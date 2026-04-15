using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PIMS.Controllers
{
    [Area("PIMS")]
    //[Authorize(Roles = "PIMSAdmin,PIMSUser")]
    //[AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
    [AllowAnonymous]
    public class ProjectDetailsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectListService _projectListService;
        private readonly IProjectDetailsService _projectDetailsService;
        private readonly IProjectCommentService _commentService;

        public ProjectDetailsController(
            IMapper mapper,
            IProjectListService projectListService,
            IProjectDetailsService projectDetailsService,
            IProjectCommentService commentService)
        {
            _mapper = mapper;
            _projectListService = projectListService;
            _projectDetailsService = projectDetailsService;
            _commentService = commentService;
        }

        public async Task<IActionResult> Index(string parentproject)
        {
            ProjectDetailsViewModel viewModel = await BuildViewModelAsync(parentproject);
            return View(viewModel);
        }

        private async Task<ProjectDetailsViewModel> BuildViewModelAsync(string parentproject)
        {
            Task<ApiResponseDto<ProjectDto>> fpsTask = _projectListService.GetFpsProjectByIdAsync(parentproject);
            Task<ApiResponseDto<ProposedProjectDto>> proposedTask = _projectDetailsService.GetProposedProjectAsync(parentproject);
            Task<ApiResponseDto<List<ProjectsDto>>> yearlyTask = _projectListService.GetYearlyDetailsByProjectAsync(parentproject);
            Task<ApiResponseDto<ProjectDetailDto>> pimsTask = _projectDetailsService.GetPimsDetailAsync(parentproject);
            Task<ApiResponseDto<List<ProjectListViewDto>>> allProjectsTask = _projectListService.GetAllProjectsListAsync();
            Task<ApiResponseDto<List<RiskDto>>> risksTask = _projectDetailsService.GetAllRiskAsync();

            await Task.WhenAll(fpsTask, proposedTask, yearlyTask, pimsTask, allProjectsTask, risksTask);

            ProposedProjectDto? proposed = proposedTask.Result.Data;
            proposed?.TransferTo = proposed.Parentproject;
            ProjectDetailDto? pimsDetail = pimsTask.Result.Data;

            PaginationFilter<string> defaultCommentRequest = new() { Filter = "{}" };
            DataGridConfig<ProjectCommentItem> commentsGrid = await BuildCommentsGridAsync(parentproject, null, defaultCommentRequest);

            return new ProjectDetailsViewModel
            {
                Parentproject = parentproject,
                FpsProjectDetails = fpsTask.Result.Data,
                YearlyDetails = yearlyTask.Result.Data ?? [],
                ProposedProjectDetails = proposed ?? new ProposedProjectDto(),
                TransferToOptions = GetTransferToOptions(allProjectsTask),
                RiskRatingOptions = GetRiskRatingOptions(risksTask),
                ProjectDetails = pimsDetail,
                Projecttitle = proposed?.Projecttitle,
                Costbookno = proposed?.Costbookno,
                Disease = proposed?.Disease,
                Program = proposed?.Program,
                Customer = proposed?.Customer,
                Manager = proposed?.Manager,
                Version = pimsDetail?.Version,
                FileRef = pimsDetail?.FileRef,
                CustomerRef = pimsDetail?.CustomerRef,
                StartDate = pimsDetail?.StartDate,
                EndDate = pimsDetail?.EndDate,
                CostbookNumber = pimsDetail?.CostbookNumber,
                Riskid = pimsDetail?.Riskid,
                UseProjectYears = pimsDetail?.UseProjectYears ?? false,
                RevisedEndDate = pimsDetail?.RevisedEndDate,
                ClosedDate = pimsDetail?.ClosedDate,
                CommentsGrid = commentsGrid,
                YearOptions = Enumerable.Range(2017, DateTime.Today.Year - 2017 + 1)
                    .Select(y => new SelectListItem(y.ToString(), y.ToString()))
                    .ToList()
            };
        }

        private static List<SelectListItem> GetRiskRatingOptions(Task<ApiResponseDto<List<RiskDto>>> risksTask)
        {
            return risksTask.Result.Data?
                            .Select(p => new SelectListItem(p.Riskrating, p.Riskid.ToString()))
                            .ToList() ?? [];
        }

        private static List<SelectListItem> GetTransferToOptions(Task<ApiResponseDto<List<ProjectListViewDto>>> allProjectsTask)
        {
            return allProjectsTask.Result.Data?
                            .Select(p => new SelectListItem(p.Parentproject, p.Parentproject))
                            .ToList() ?? [];
        }

        [HttpPost]
        public async Task<IActionResult> LoadCommentsGrid(string parentproject, int? year, PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            DataGridConfig<ProjectCommentItem> gridConfig = await BuildCommentsGridAsync(parentproject, year, request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ProjectCommentItem>> BuildCommentsGridAsync(
            string parentproject, int? year, PaginationFilter<string> request)
        {
            Dictionary<string, string> filterDict =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new();

            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            ApiResponseDto<List<CommentDto>> pagedData =
                await _commentService.GetCommentsByProjectAsync(parentproject, year, queryParameters);

            List<ProjectCommentItem> items = pagedData.Data is not null
                ? _mapper.Map<List<ProjectCommentItem>>(pagedData.Data)
                : new List<ProjectCommentItem>();

            PaginationModel paginationModel = pagedData.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<ProjectCommentItem>
            {
                GridId = "projectCommentsGrid",
                Title = string.Empty,
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Commentno",
                AllowAdd= false,
                EditFunction = "editComment",
                DeleteFunction = "deleteComment",
                ExtraFilterMethod = "getProjectDetailsExtraFilters",
                BindGridUrl = "/PIMS/ProjectDetails/LoadCommentsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProjectCommentItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePimsDetail(string parentproject, [FromBody] ProjectDetailDto dto)
        {
            dto.Parentproject = parentproject;
            ApiResponseDto<ProjectDetailDto> result =
                await _projectDetailsService.SavePimsDetailAsync(parentproject, dto);
            return result.Success
                ? Json(new { success = true, data = result.Data, message = "PIMS details saved successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProposedProject(string parentproject, ProjectDetailsViewModel projectDetailsViewModel)
        {
            if (projectDetailsViewModel?.ProposedProjectDetails == null)
            {
                return Json(new { success = false, errors = new[] { "Project details are required" } });
            }

            projectDetailsViewModel.ProposedProjectDetails.Parentproject = parentproject;

            await _projectDetailsService.UpdateProposedProjectAsync(parentproject, projectDetailsViewModel.ProposedProjectDetails);

            return RedirectToAction(nameof(Index), new { parentproject });
        }



        [HttpGet]
        public async Task<IActionResult> GetComment(int commentno)
        {
            ApiResponseDto<CommentDto> result = await _commentService.GetByIdAsync(commentno);

            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Comment added successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment([FromBody] CommentDto dto)
        {
            dto.Commenttext = dto.Comment?.Trim();
            ApiResponseDto<CommentDto> result = await _commentService.CreateCommentAsync(dto);
            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Comment added successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateComment([FromBody] CommentDto dto)
        {
            ApiResponseDto<CommentDto> result = await _commentService.UpdateCommentAsync(dto.Commentno, dto);
            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Comment updated successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteComment(int commentno)
        {
            ApiResponseDto<bool> result = await _commentService.DeleteCommentAsync(commentno);
            return result.Success
                ? Json(new { success = true, message = "Comment deleted successfully" })
                : Json(new { success = false, errors = result.Errors });
        }
    }
}
