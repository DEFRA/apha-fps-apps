using Apha.Common.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Apha.FPS.Api.Filters
{
    public class ApiResponseActionFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(
            ResultExecutingContext context,
            ResultExecutionDelegate next)
        {
            if (context.Result is not ObjectResult objectResult ||
                objectResult.Value is null)
            {
                await next();
                return;
            }

            var correlationId = GetCorrelationId(context);
            var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
            var isSuccess = statusCode >= 200 && statusCode < 300;

            object wrappedResponse = IsPaginatedResult(objectResult.Value)
                ? CreatePaginatedResponse(objectResult.Value, correlationId, isSuccess)
                : CreateStandardResponse(objectResult.Value, correlationId, isSuccess);

            context.Result = new ObjectResult(wrappedResponse)
            {
                StatusCode = statusCode
            };

            await next();
        }

        private static bool IsPaginatedResult(object value)
        {
            var type = value.GetType();

            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(PaginationRes<>);
        }

        private static object CreateStandardResponse(object value, string correlationId, bool isSuccess)
        {
            // Check if the value already has the error response structure (anonymous objects from BadRequest)
            var valueType = value.GetType();
            var successProp = valueType.GetProperty("success");
            var messageProp = valueType.GetProperty("message");
            var errorsProp = valueType.GetProperty("errors");

            if (successProp != null && messageProp != null && errorsProp != null)
            {
                // This is already an error response with success, message, and errors
                // Wrap it in ApiResponse but preserve the structure
                dynamic dynValue = value;
                return new ApiResponse<object>
                {
                    Success = dynValue.success,
                    Data = null,
                    Errors = ConvertToApiErrors(dynValue.errors),
                    Meta = new ApiMeta
                    {
                        CorrelationId = correlationId,
                        TimestampUtc = DateTime.UtcNow,
                        Message = dynValue.message
                    }
                };
            }

            // Standard success response
            return new ApiResponse<object>
            {
                Success = isSuccess,
                Data = value,
                Errors = null,
                Meta = CreateMeta(correlationId)
            };
        }

        private static List<ApiError>? ConvertToApiErrors(dynamic errors)
        {
            if (errors == null) return null;

            var errorList = new List<ApiError>();
            foreach (var error in errors)
            {
                errorList.Add(new ApiError
                {
                    Code = error.code?.ToString(),
                    Message = error.message?.ToString()
                });
            }
            return errorList.Count > 0 ? errorList : null;
        }

        private static object CreatePaginatedResponse(object value, string correlationId, bool isSuccess)
        {
            dynamic paginated = value;

            return new ApiResponse<object>
            {
                Success = isSuccess,
                Data = paginated.Data,
                Pagination = paginated.PaginationData,
                Errors = null,
                Meta = CreateMeta(correlationId)
            };
        }

        private static ApiMeta CreateMeta(string correlationId)
        {
            return new ApiMeta
            {
                CorrelationId = correlationId,
                TimestampUtc = DateTime.UtcNow
            };
        }

        private static string GetCorrelationId(ResultExecutingContext context)
        {
            return context.HttpContext.Request.Headers["X-Correlation-ID"]
                   .ToString();
        }
    }
}
