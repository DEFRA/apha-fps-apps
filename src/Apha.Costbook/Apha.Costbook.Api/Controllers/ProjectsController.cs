using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Apha.Costbook.Api.Controllers
{
        [ApiController]
        [Route("api/projects")]
        public class ProjectsController : ControllerBase
    {
            private readonly IProjectService _service;
            private readonly IContractService _contractService;
            private readonly IDiseaseService _diseaseService;
            private readonly IProgramService _programService;
            private readonly ICustomerService _customerService;
            private readonly IStaffService _staffService;
        private readonly IMapper _mapper;

        public ProjectsController(IProjectService service, IContractService contractService,IDiseaseService diseaseService,IProgramService programService,ICustomerService customerService,
                IStaffService staffService, IMapper mapper)
            {
                _service = service;
                _contractService = contractService;
                _diseaseService = diseaseService;
                _programService = programService;
                _customerService = customerService;
                _staffService = staffService;
                _mapper = mapper;
        }
        [HttpGet("paginated")]
        public async Task<IActionResult> GetPaginatedProjectsAsync([FromQuery] PaginationReq<string> query)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _service.GetPaginatedProjectsAsync(filter);
            return Ok(_mapper.Map<PaginationRes<ProjectRes>>(result));
        }        


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(string id)
        {
            var project = await _service.GetProjectByIdAsync(id);
            if (project == null) return NotFound();
            return Ok(_mapper.Map<ProjectRes>(project));
        }

        [HttpPost]
        public async Task<IActionResult> AddProject([FromBody] ProjectReq projectReq)
        {
            var projectDto = _mapper.Map<ProjectDto>(projectReq);
            var result = await _service.AddProjectAsync(projectDto);
            return CreatedAtAction(nameof(GetProject), new { id = result.ProjectId }, _mapper.Map<ProjectRes>(result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(string id, [FromBody] ProjectReq projectReq)
        {
            try
            {
                var projectDto = _mapper.Map<ProjectDto>(projectReq);
                var result = await _service.UpdateProjectAsync(id, projectDto);
                return Ok(_mapper.Map<ProjectRes>(result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the project", details = ex.Message });
            }
        }



        [HttpDelete("{id}/delete")]
        public async Task<IActionResult> DeleteProject(string id)
        {
           
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { message = "Project ID is required" });
                }

                var success = await _service.DeleteProjectAsync(id);

                if (success)
                {
                    return Ok(true); // Return just the boolean
                   
                }
                else
                {
                    return NotFound(false); // Return false for not found
                    
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, false); // Return false for errors
            }
        }

        [HttpPost("{id}/copy")]
        public async Task<IActionResult> CopyProject(string id, [FromBody] string newId)
        {
            try
            {
                var result = await _service.CopyProjectAsync(id, newId);
                return CreatedAtAction(nameof(GetProject), new { id = result.ProjectId }, _mapper.Map<ProjectRes>(result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while copying the project", details = ex.Message });
            }
        }

        [HttpPost("{id}/recost")]
        public async Task<IActionResult> RecostProject(string id)
        {
            var success = await _service.RecostProjectAsync(id);
            return Ok(success);
        }


        [HttpGet("programs")]
        public async Task<IActionResult> GetPrograms()
        {
            try
            {
                var programs = await _programService.GetAllProgramsAsync();
                var mappedPrograms = _mapper.Map<List<ProgramRes>>(programs);

                var response = new ApiResponse<List<ProgramRes>>
                {
                    Success = true,
                    Data = mappedPrograms,
                    Errors = new List<ApiError>(),  // do not set null
                    Meta = new ApiMeta()            // do not set null
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<List<ProgramRes>>
                {
                    Success = false,
                    Data = null,
                    Errors = new List<ApiError>
            {
                new ApiError
                {
                    Code = "INTERNAL_ERROR",
                    Message = "Failed to retrieve programs",
                    Details = ex.Message
                }
            },
                    Meta = new ApiMeta()  // provide empty object
                };

                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("number")]
        public async Task<IActionResult> GetNextProjectNumber([FromQuery] string? baseNumber)
        {
            try
            {
                var number = await _service.GetNextProjectNumberAsync(baseNumber);
                var response = new ApiResponse<string>
                {
                    Success = true,
                    Data = number,
                    Errors = new List<ApiError>(),
                    Meta = new ApiMeta()
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>
                {
                    Success = false,
                    Data = null,
                    Errors = new List<ApiError>
            {
                new ApiError
                {
                    Code = "INTERNAL_ERROR",
                    Message = "Failed to retrieve project number",
                    Details = ex.Message
                }
            },
                    Meta = new ApiMeta()
                };
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("contracts")]
        public async Task<IActionResult> GetContracts()
        {
            try
            {
                var contracts = await _contractService.GetAllContractNumbersAsync();
                var mappedContracts = contracts.Select(contractNumber => new ContractRes
                {
                    ContractNumber = contractNumber
                }).ToList();

                var response = new ApiResponse<List<ContractRes>>
                {
                    Success = true,
                    Data = mappedContracts,
                    Errors = new List<ApiError>(),
                    Meta = new ApiMeta()
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<List<ContractRes>>
                {
                    Success = false,
                    Data = null,
                    Errors = new List<ApiError>
            {
                new ApiError
                {
                    Code = "INTERNAL_ERROR",
                    Message = "Failed to retrieve contracts",
                    Details = ex.Message
                }
            },
                    Meta = new ApiMeta()
                };
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("diseases")]
        public async Task<IActionResult> GetDiseases()
        {
            try
            {
                var diseases = await _diseaseService.GetAllDiseasesAsync();
                var mappedDiseases = _mapper.Map<List<DiseaseRes>>(diseases);

                var response = new ApiResponse<List<DiseaseRes>>
                {
                    Success = true,
                    Data = mappedDiseases,
                    Errors = new List<ApiError>(),
                    Meta = new ApiMeta()
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<List<DiseaseRes>>
                {
                    Success = false,
                    Data = null,
                    Errors = new List<ApiError>
            {
                new ApiError
                {
                    Code = "INTERNAL_ERROR",
                    Message = "Failed to retrieve diseases",
                    Details = ex.Message
                }
            },
                    Meta = new ApiMeta()
                };
                return StatusCode(500, errorResponse);
            }
        }

        
        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                var mappedCustomers = _mapper.Map<List<CustomerRes>>(customers);

                var response = new ApiResponse<List<CustomerRes>>
                {
                    Success = true,
                    Data = mappedCustomers,
                    Errors = new List<ApiError>(),
                    Meta = new ApiMeta()
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<List<CustomerRes>>
                {
                    Success = false,
                    Data = null,
                    Errors = new List<ApiError>
            {
                new ApiError
                {
                    Code = "INTERNAL_ERROR",
                    Message = "Failed to retrieve customers",
                    Details = ex.Message
                }
            },
                    Meta = new ApiMeta()
                };
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("staff")]
        public async Task<IActionResult> GetStaff()
        {
            try
            {
                var staff = await _staffService.GetAllStaffAsync();
                var mappedStaff = _mapper.Map<List<StaffRes>>(staff);

                var response = new ApiResponse<List<StaffRes>>
                {
                    Success = true,
                    Data = mappedStaff,
                    Errors = new List<ApiError>(),
                    Meta = new ApiMeta()
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<List<StaffRes>>
                {
                    Success = false,
                    Data = null,
                    Errors = new List<ApiError>
            {
                new ApiError
                {
                    Code = "INTERNAL_ERROR",
                    Message = "Failed to retrieve staff",
                    Details = ex.Message
                }
            },
                    Meta = new ApiMeta()
                };
                return StatusCode(500, errorResponse);
            }
        }
    }
}
