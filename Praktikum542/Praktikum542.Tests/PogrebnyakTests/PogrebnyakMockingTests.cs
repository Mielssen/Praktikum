using Xunit;
using Moq;
using System.Collections.Generic;
using Praktikum542.DTOs;
using Praktikum542.Services;
using Praktikum542.Repositories;

namespace Praktikum542.Tests.PogrebnyakTests
{
    public class PogrebnyakMockingTests
    {
        private readonly Mock<ITourRepository> _mockRepo;

        public PogrebnyakMockingTests()
        {
            _mockRepo = new Mock<ITourRepository>();
        }

        [Fact]
        public void GetById_CallsRepositoryOnce()
        {
            _mockRepo.Setup(r => r.GetById(1)).Returns(new Tour { TourId = 1, Name = "Тестовий Тур" });

            var result = _mockRepo.Object.GetById(1);

            Assert.NotNull(result);
            Assert.Equal("Тестовий Тур", result.Name);
            _mockRepo.Verify(r => r.GetById(1), Times.Once);
        }

        [Fact]
        public void GetAll_ReturnsMockedList()
        {
            var mockTours = new List<Tour>
            {
                new Tour { TourId = 1, Name = "Тур 1" },
                new Tour { TourId = 2, Name = "Тур 2" }
            };
            _mockRepo.Setup(r => r.GetAll()).Returns(mockTours);

            var tours = _mockRepo.Object.GetAll();

            Assert.Equal(2, tours.Count);
            _mockRepo.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public void Delete_CallsRepositoryRemove()
        {
            var tour = new Tour { TourId = 5, Name = "Тур на видалення" };

            _mockRepo.Object.Delete(tour);

            _mockRepo.Verify(r => r.Delete(tour), Times.Once);
        }

        [Fact]
        public void Search_WithQuery_ReturnsFilteredResult()
        {
            _mockRepo.Setup(r => r.Search("Карпати"))
                     .Returns(new List<Tour> { new Tour { TourId = 10, Name = "Карпати Казка" } });

            var results = _mockRepo.Object.Search("Карпати");

            Assert.Single(results);
            Assert.Contains("Карпати", results[0].Name);
        }

        [Fact]
        public void Add_NewTour_InvokesAddMethod()
        {
            var newTour = new Tour { Name = "Новий Тур" };

            _mockRepo.Object.Add(newTour);

            _mockRepo.Verify(r => r.Add(It.IsAny<Tour>()), Times.Once);
        }
    }

    public interface ITourRepository
    {
        Tour GetById(int id);
        List<Tour> GetAll();
        List<Tour> Search(string query);
        void Add(Tour tour);
        void Delete(Tour tour);
    }

    public class Tour
    {
        public int TourId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}