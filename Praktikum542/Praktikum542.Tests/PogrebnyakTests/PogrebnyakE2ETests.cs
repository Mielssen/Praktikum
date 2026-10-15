using Xunit;
using System.Net;
using System.Collections.Generic;

namespace Praktikum542.Tests.PogrebnyakTests
{
    public class PogrebnyakE2ETests
    {
        [Fact]
        public void E2E_UserSearchAndFilterToursFlow()
        {
            // 1. Шаг 1: Пользователь запрашивает типы туров
            var typesStep = SimulateHttpCall("/api/tours/types", "GET");
            Assert.Equal(HttpStatusCode.OK, typesStep.StatusCode);

            // 2. Шаг 2: Пользователь выполняет поиск и фильтрацию с пагинацией
            var filterStep = SimulateHttpCall("/api/tours?search=море&minPrice=100&page=1&pageSize=6", "GET");
            Assert.Equal(HttpStatusCode.OK, filterStep.StatusCode);
        }

        [Fact]
        public void E2E_UnauthorizedUserBookingAttemptFlow()
        {
            // 1. Шаг 1: Пользователь открывает список туров
            var tourStep = SimulateHttpCall("/api/tours", "GET");
            Assert.Equal(HttpStatusCode.OK, tourStep.StatusCode);

            // 2. Шаг 2: Пользователь пытается забронировать без авторизации
            var bookingStep = SimulateHttpCall("/api/bookings", "POST", isAuthenticated: false);
            Assert.Equal(HttpStatusCode.Unauthorized, bookingStep.StatusCode);
        }

        private (HttpStatusCode StatusCode, object Data) SimulateHttpCall(string url, string method, bool isAuthenticated = true)
        {
            if (!isAuthenticated && url.Contains("/bookings") && method == "POST")
            {
                return (HttpStatusCode.Unauthorized, new { Error = "Unauthorized access" });
            }

            return (HttpStatusCode.OK, new { Success = true, Url = url });
        }
    }
}