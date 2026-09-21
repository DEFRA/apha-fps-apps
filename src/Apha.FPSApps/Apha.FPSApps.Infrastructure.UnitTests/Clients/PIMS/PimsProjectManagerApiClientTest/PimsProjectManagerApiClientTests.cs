using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using MapsterMapper;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsProjectManagerApiClientTest
{
    public class PimsProjectManagerApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsProjectManagerApiClient _client;

        public PimsProjectManagerApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsProjectManagerApiClient(_http, _mapper);
        }

        [Fact]
        public void Constructor_WithValidDependencies_InitializesClient()
        {
            var client = new PimsProjectManagerApiClient(_http, _mapper);

            Assert.NotNull(client);
        }

        [Fact]
        public async Task GetAllProjectManagersAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var response = new ApiResponse<List<ProjectManagerRes>>
            {
                Success = true,
                Data = [new ProjectManagerRes { ProjectManager = "J. Smith", Email = "jsmith@apha.gov.uk" }]
            };
            var mapped = ApiResponseDto<List<ProjectManagerDto>>.SuccessResponse(
                [new ProjectManagerDto { Projectmanager = "J. Smith", Email = "jsmith@apha.gov.uk" }]);

            _http.GetAsync<List<ProjectManagerRes>>(Arg.Any<string>()).Returns(response);
            _mapper.Map<ApiResponseDto<List<ProjectManagerDto>>>(response).Returns(mapped);

            var result = await _client.GetAllProjectManagersAsync(query);

            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<ProjectManagerRes>>(Arg.Is<string>(u => u.Contains(PimsApiEndpoints.GetAllProjectManagers)));
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectManagerDto>>>(response);
        }

        [Fact]
        public async Task GetAllProjectManagersAsync_WhenHttpThrows_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<ProjectManagerRes>>(Arg.Any<string>()).Throws(new Exception("Network failure"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.GetAllProjectManagersAsync(query));

            Assert.Equal("Network failure", ex.Message);
        }

        [Fact]
        public async Task GetManagerNamesAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            var response = new ApiResponse<List<string>>
            {
                Success = true,
                Data = ["J. Smith", "A. Jones"]
            };
            var mapped = ApiResponseDto<List<string>>.SuccessResponse(["J. Smith", "A. Jones"]);

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectManagerNames).Returns(response);
            _mapper.Map<ApiResponseDto<List<string>>>(response).Returns(mapped);

            var result = await _client.GetManagerNamesAsync();

            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _http.Received(1).GetAsync<List<string>>(PimsApiEndpoints.GetProjectManagerNames);
        }

        [Fact]
        public async Task GetManagerNamesAsync_WhenHttpThrows_PropagatesException()
        {
            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectManagerNames).Throws(new Exception("Network failure"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.GetManagerNamesAsync());

            Assert.Equal("Network failure", ex.Message);
        }

        [Fact]
        public async Task GetPagedProjectManagersAsync_WhenSuccessResponse_ReturnsPagedResult()
        {
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var response = new ApiResponse<List<ProjectManagerRes>>
            {
                Success = true,
                Data = [new ProjectManagerRes { ProjectManager = "J. Smith" }],
                Pagination = new Pagination { PageNumber = 2, PageSize = 5, TotalRecords = 11, TotalPages = 3 }
            };
            var mappedItems = new List<ProjectManagerDto>
            {
                new ProjectManagerDto { Projectmanager = "J. Smith" }
            };

            _http.GetAsync<List<ProjectManagerRes>>(Arg.Any<string>()).Returns(response);
            _mapper.Map<List<ProjectManagerDto>>(response.Data!).Returns(mappedItems);

            var result = await _client.GetPagedProjectManagersAsync(query);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(11, result.Data.TotalCount);
            Assert.Equal(2, result.Data.PageNumber);
            Assert.Equal(5, result.Data.PageSize);
            Assert.Single(result.Data.data);
        }

        [Fact]
        public async Task GetPagedProjectManagersAsync_WhenHttpThrows_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<ProjectManagerRes>>(Arg.Any<string>()).Throws(new Exception("Network failure"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.GetPagedProjectManagersAsync(query));

            Assert.Equal("Network failure", ex.Message);
        }

        [Fact]
        public async Task GetProjectManagerByNameAsync_WhenHttpThrows_PropagatesException()
        {
            _http.GetAsync<ProjectManagerRes>(Arg.Any<string>()).Throws(new Exception("Network failure"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.GetProjectManagerByNameAsync("J. Smith"));

            Assert.Equal("Network failure", ex.Message);
        }

        [Fact]
        public async Task CreateProjectManagerAsync_WhenHttpThrows_PropagatesException()
        {
            var dto = new ProjectManagerDto { Projectmanager = "J. Smith" };
            var request = new ProjectManagerReq { ProjectManager = "J. Smith" };

            _mapper.Map<ProjectManagerReq>(dto).Returns(request);
            _http.PostAsync<ProjectManagerReq, ProjectManagerRes>(PimsApiEndpoints.CreateProjectManager, request)
                .Throws(new Exception("Network failure"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.CreateProjectManagerAsync(dto));

            Assert.Equal("Network failure", ex.Message);
        }

        [Fact]
        public async Task UpdateProjectManagerAsync_WhenHttpThrows_PropagatesException()
        {
            var dto = new ProjectManagerDto { Projectmanager = "J. Smith" };
            var request = new ProjectManagerReq { ProjectManager = "J. Smith" };

            _mapper.Map<ProjectManagerReq>(dto).Returns(request);
            _http.PutAsync<ProjectManagerReq, ProjectManagerRes>(Arg.Any<string>(), request)
                .Throws(new Exception("Network failure"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.UpdateProjectManagerAsync("J. Smith", dto));

            Assert.Equal("Network failure", ex.Message);
        }

        [Fact]
        public async Task DeleteProjectManagerAsync_WhenHttpThrows_PropagatesException()
        {
            _http.DeleteAsync<bool>(Arg.Any<string>()).Throws(new Exception("Network failure"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _client.DeleteProjectManagerAsync("J. Smith"));

            Assert.Equal("Network failure", ex.Message);
        }
    }
}
