using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Integrations.HttpExecutor
{
    public class HttpResponseExtensionsTests
    {
        [Fact]
        public async Task ToApiResponse_Deserializes_List_Data_Response()
        {
            // Arrange
            var json = JsonSerializer.Serialize(new ApiResponse<List<WorkGroupEmployeeRes>>
            {
                Success = true,
                Data = new List<WorkGroupEmployeeRes>
                {
                    new WorkGroupEmployeeRes
                    {
                        PactId = "P001",
                        SpNumber = "S001",
                        WorkGroupGrade = "WG1",
                        Name = "Test Person",
                        PersonStatus = "Active",
                        HrsPaid = 37.5,
                        Leave = 0,
                        SickSpecial = 0,
                        HrsAvail = 37.5,
                        MakeAvailable = 1,
                        TimeRecorder = 0
                    }
                },
                Pagination = new Pagination
                {
                    PageNumber = 1,
                    PageSize = 10,
                    TotalPages = 1,
                    TotalRecords = 1
                },
                Meta = new ApiMeta
                {
                    CorrelationId = "corr-1",
                    TimestampUtc = DateTime.UtcNow
                }
            }, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            using var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // Act
            var result = await response.ToApiResponse<List<WorkGroupEmployeeRes>>();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data!);
            Assert.Equal("P001", result.Data![0].PactId);
            Assert.NotNull(result.Pagination);
            Assert.Equal(1, result.Pagination!.TotalRecords);
        }

        [Fact]
        public async Task ToApiResponse_WhenNoContent_Throws()
        {
            // An unexpected 204 through the shared executor is never silently treated as
            // success — this is the invariant the allowNoContent revert restores.
            using var response = new HttpResponseMessage(HttpStatusCode.NoContent);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => response.ToApiResponse<BulkRatesQueueEntryRes>());
        }
    }
}
