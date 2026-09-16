using Apha.Common.Contracts;
using Apha.FPS.Api.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Apha.FPS.Api.UnitTests.Filters
{
    public class ApiResponseActionFilterTests
    {
        private static ResultExecutingContext CreateContext(IActionResult result)
        {
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor());

            return new ResultExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                result,
                controller: new object());
        }

        private static async Task ExecuteAsync(ResultExecutingContext context)
        {
            var filter = new ApiResponseActionFilter();

            await filter.OnResultExecutionAsync(context, () =>
                Task.FromResult(new ResultExecutedContext(
                    context,
                    context.Filters,
                    context.Result,
                    context.Controller)));
        }

        [Fact]
        public async Task OnResultExecutionAsync_WrapsNonNullValue()
        {
            // Arrange
            var context = CreateContext(new OkObjectResult(123.45m));

            // Act
            await ExecuteAsync(context);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(context.Result);
            var response = Assert.IsType<ApiResponse<object>>(objectResult.Value);

            Assert.True(response.Success);
            Assert.Equal(123.45m, response.Data);
        }

        [Fact]
        public async Task OnResultExecutionAsync_LeavesNonObjectResultUntouched()
        {
            // Arrange
            var result = new NoContentResult();
            var context = CreateContext(result);

            // Act
            await ExecuteAsync(context);

            // Assert
            Assert.Same(result, context.Result);
        }
    }
}
