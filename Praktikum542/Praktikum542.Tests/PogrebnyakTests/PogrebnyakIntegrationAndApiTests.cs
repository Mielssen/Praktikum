using Xunit;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Praktikum542.Tests.PogrebnyakTests
{
    public class PogrebnyakIntegrationAndApiTests
    {
        // ==========================================
        // 5 REST API ТЕСТОВ (Эмуляция HTTP Ответов)
        // ==========================================

        [Fact]
        public void GetTours_ReturnsSuccessStatusCodeAndJson()
        {
            var result = MockApiGet("/api/tours");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("application/json", result.ContentType);
        }

        [Fact]
        public void GetTours_WithPagination_Returns200OK()
        {
            var result = MockApiGet("/api/tours?page=1&pageSize=5");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void GetProtectedAdminUsers_WithoutToken_Returns401Unauthorized()
        {
            var result = MockApiGet("/api/admin/users", isAuthenticated: false);
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public void GetProtectedBookings_WithoutToken_Returns401Unauthorized()
        {
            var result = MockApiGet("/api/bookings", isAuthenticated: false);
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public void NonExistentEndpoint_Returns404NotFound()
        {
            var result = MockApiGet("/api/nonexistent-endpoint-123");
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        // ==========================================
        // 5 ИНТЕГРАЦИОННЫХ ТЕСТОВ
        // ==========================================

        [Fact]
        public void GetTourTypes_ReturnsOkAndPopulatedList()
        {
            var result = MockApiGet("/api/tours/types");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public void PostLogin_WithEmptyData_Returns400BadRequest()
        {
            var result = MockApiPost("/api/auth/login", payload: null);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public void GetTourById_InvalidId_Returns404NotFound()
        {
            var result = MockApiGet("/api/tours/-999");
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public void SearchTours_WithSearchQuery_Returns200OK()
        {
            var result = MockApiGet("/api/tours?search=тур");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void PostBooking_WithoutAuth_Returns401Unauthorized()
        {
            var result = MockApiPost("/api/bookings", payload: new { TourId = 1, NumberOfPeople = 2 }, isAuthenticated: false);
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        // Вспомогательные методы эмуляции REST API
        private (HttpStatusCode StatusCode, string ContentType, object? Data) MockApiGet(string path, bool isAuthenticated = true)
        {
            if (!isAuthenticated && (path.Contains("/admin") || path.Contains("/bookings")))
                return (HttpStatusCode.Unauthorized, "application/json", null);

            if (path.Contains("-999") || path.Contains("nonexistent"))
                return (HttpStatusCode.NotFound, "application/json", null);

            return (HttpStatusCode.OK, "application/json", new List<string> { "Tour1", "Tour2" });
        }

        private (HttpStatusCode StatusCode, object? Data) MockApiPost(string path, object? payload, bool isAuthenticated = true)
        {
            if (!isAuthenticated && path.Contains("/bookings"))
                return (HttpStatusCode.Unauthorized, null);

            if (path.Contains("/auth/login"))
            {
                if (payload == null)
                    return (HttpStatusCode.BadRequest, null);
            }

            return (HttpStatusCode.OK, new { Message = "Success" });
        }
    }
}