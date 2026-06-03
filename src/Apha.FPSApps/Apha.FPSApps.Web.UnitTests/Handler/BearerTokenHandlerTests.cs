using Apha.FPSApps.Web.Handler;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;
using NSubstitute;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Apha.FPSApps.Web.UnitTests.Handler
{
    public class BearerTokenHandlerTests
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string[] _scopes;

        public BearerTokenHandlerTests()
        {
            _tokenAcquisition = Substitute.For<ITokenAcquisition>();
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _scopes = ["api://test-scope/.default"];
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenScopesIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BearerTokenHandler(_tokenAcquisition, _httpContextAccessor, null!));
        }

        [Fact]
        public void Constructor_DoesNotThrow_WhenAllArgumentsProvided()
        {
            // Act & Assert (no exception)
            var handler = new BearerTokenHandler(_tokenAcquisition, _httpContextAccessor, _scopes);
            Assert.NotNull(handler);
        }

        [Fact]
        public async Task SendAsync_ThrowsUnauthorizedException_WhenHttpContextIsNull()
        {
            // Arrange
            _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
            var handler = new BearerTokenHandler(_tokenAcquisition, _httpContextAccessor, _scopes);
            var invoker = CreateInvoker(handler, new HttpResponseMessage(HttpStatusCode.OK));

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.com"), CancellationToken.None));
        }

        [Fact]
        public async Task SendAsync_ThrowsUnauthorizedException_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var identity = new ClaimsIdentity(); // not authenticated
            var user = new ClaimsPrincipal(identity);
            var httpContext = Substitute.For<HttpContext>();
            httpContext.User.Returns(user);
            _httpContextAccessor.HttpContext.Returns(httpContext);

            var handler = new BearerTokenHandler(_tokenAcquisition, _httpContextAccessor, _scopes);
            var invoker = CreateInvoker(handler, new HttpResponseMessage(HttpStatusCode.OK));

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.com"), CancellationToken.None));
        }

        [Fact]
        public async Task SendAsync_SetsAuthorizationHeader_WhenUserIsAuthenticated()
        {
            // Arrange
            const string accessToken = "test-access-token";
            var identity = new ClaimsIdentity("Bearer");
            var user = new ClaimsPrincipal(identity);
            var httpContext = Substitute.For<HttpContext>();
            httpContext.User.Returns(user);
            _httpContextAccessor.HttpContext.Returns(httpContext);

            _tokenAcquisition
                .GetAccessTokenForUserAsync(_scopes, user: user)
                .Returns(accessToken);

            var handler = new BearerTokenHandler(_tokenAcquisition, _httpContextAccessor, _scopes);
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var invoker = CreateInvoker(handler, expectedResponse);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            var response = await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal(accessToken, request.Headers.Authorization?.Parameter);
            await _tokenAcquisition.Received(1).GetAccessTokenForUserAsync(_scopes, user: user);
        }

        /// <summary>
        /// Creates an <see cref="HttpMessageInvoker"/> that wraps the handler under test
        /// with a stub inner handler that returns the given response.
        /// </summary>
        private static HttpMessageInvoker CreateInvoker(BearerTokenHandler handler, HttpResponseMessage response)
        {
            handler.InnerHandler = new StubHandler(response);
            return new HttpMessageInvoker(handler);
        }

        private sealed class StubHandler(HttpResponseMessage response) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(response);
        }
    }
}
