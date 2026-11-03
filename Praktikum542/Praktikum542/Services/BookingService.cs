using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Exceptions;
using Praktikum542.DTOs;
using Praktikum542.Common;

namespace Praktikum542.Services
{
    public class BookingService
    {
        private readonly BookingRepository _repo;
        private readonly PraktikumContext _context;
        private readonly ILogger<BookingService> _logger;
        public BookingService(BookingRepository repo, PraktikumContext context, ILogger<BookingService> logger)
        {
            _repo = repo;
            _context = context;
            _logger = logger;
        }

        public void Create(int credentialId, CreateBookingDto dto)
        {
            var tourExists = _repo.TourExists(dto.TourId);
            if (!tourExists)
                throw new AppException("NOT_FOUND", "Тур не знайдено");

            var tour = _repo.GetTourById(dto.TourId);

            var startDate = dto.StartDate;
            var endDate = startDate.AddDays(tour.DurationDays);
            var availableFrom = tour.AvailableFrom;
            var availableTo = tour.AvailableTo;

            if (startDate < availableFrom)
                throw new AppException("INVALID_DATE", $"Дата початку не може бути раніше {availableFrom}");

            if (endDate > availableTo)
                throw new AppException("INVALID_DATE", $"Тур виходить за межі доступного періоду (до {availableTo})");

            if (dto.Persons.Count != dto.NumberOfAdults + dto.NumberOfChildren)
                throw new AppException("INVALID_PERSONS", "Кількість учасників не співпадає з кількістю людей");

            foreach (var person in dto.Persons)
            {
                if (string.IsNullOrWhiteSpace(person.Name))
                    throw new AppException("INVALID_PERSON", "Ім'я учасника обов'язкове");

                if (!System.Text.RegularExpressions.Regex.IsMatch(person.Name, @"^[a-zA-Zа-яА-ЯіІїЇєЄ\s]+$"))
                    throw new AppException("INVALID_PERSON", "Ім'я має містити тільки літери");

                if (string.IsNullOrWhiteSpace(person.PassportData))
                    throw new AppException("INVALID_PERSON", "Паспортні дані обов'язкові");

                if (!System.Text.RegularExpressions.Regex.IsMatch(person.PassportData, @"^\d+$"))
                    throw new AppException("INVALID_PERSON", "Паспортні дані мають містити тільки цифри");

                if (person.DateOfBirth == null)
                    throw new AppException("INVALID_PERSON", "Дата народження обов'язкова");


                if (dto.NumberOfAdults < 0 || dto.NumberOfChildren < 0)
                    throw new AppException(
                        "INVALID_PERSONS",
                        "Кількість дорослих і дітей не може бути від'ємною"
                    );

                if (dto.NumberOfAdults + dto.NumberOfChildren <= 0)
                    throw new AppException(
                        "INVALID_PERSONS",
                        "У бронюванні має бути хоча б один учасник"
                    );

                if (dto.Persons == null ||
                    dto.Persons.Count != dto.NumberOfAdults + dto.NumberOfChildren)
                {
                    throw new AppException(
                        "INVALID_PERSONS",
                        "Кількість учасників не співпадає з кількістю людей"
                    );
                }

                if (dto.Persons.Count(p => p.IsChild) != dto.NumberOfChildren)
                    throw new AppException(
                        "INVALID_PERSONS",
                        "Кількість дітей не співпадає з даними учасників"
                    );

            }

            const decimal childDiscount = 0.5m;
            var totalPrice = (dto.NumberOfAdults * tour.Price) +
                             (dto.NumberOfChildren * tour.Price * childDiscount);

            var booking = new Booking
            {
                CredentialId = credentialId,
                TourId = dto.TourId,
                StartDate = startDate,
                BookingDate = DateTime.Now,
                Status = BookingStatuses.Pending,
                NumberOfPeople = dto.NumberOfAdults + dto.NumberOfChildren,
                TotalPrice = totalPrice,
                Comment = dto.Comment
            };

            _repo.Add(booking);

            foreach (var person in dto.Persons)
            {
                _context.BookingPersons.Add(new BookingPerson
                {
                    BookingId = booking.BookingId,
                    Name = person.Name,
                    PassportData = person.PassportData,
                    DateOfBirth = person.DateOfBirth,
                    IsChild = person.IsChild
                });
            }
            _context.SaveChanges();
            _logger.LogInformation(
                "Нове бронювання: UserID={UserId}, TourID={TourId}, People={People}, Total={Total}",
                credentialId, dto.TourId, dto.NumberOfAdults + dto.NumberOfChildren, totalPrice);
        }

        public List<BookingDto> GetMy(int credentialId)
        {
            var bookings = _repo.GetByCredentialId(credentialId);
            return bookings.Select(MapToDto).ToList();
        }

        public void UpdateStatus(int bookingId, string status)
        {
            if (status != BookingStatuses.Pending &&
                status != BookingStatuses.Confirmed &&
                status != BookingStatuses.Cancelled)
            {
                throw new AppException(
                    "INVALID_STATUS",
                    "Статус має бути pending, confirmed або cancelled"
                );
            }

            var booking = _repo.GetById(bookingId);

            if (booking == null)
                throw new AppException("NOT_FOUND", "Бронювання не знайдено");

            booking.Status = status;
            _repo.Update(booking);

            _logger.LogInformation(
                "Статус бронювання ID={BookingId} змінено на {Status}",
                bookingId,
                status
            );
        }
        public PagedResult<BookingDto> GetAll(string? status,int page,int pageSize)
        {
            if (page < 1)
                page = 1;

            if (pageSize < 1)
                pageSize = 10;

            var result = _repo.GetAllPaged(status, page, pageSize);

            return new PagedResult<BookingDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = result.TotalItems,
                TotalPages = (int)Math.Ceiling(
                    (double)result.TotalItems / pageSize),
                Items = result.Items.Select(MapToDto).ToList()
            };
        }
        private static BookingDto MapToDto(Booking b)
        {
            return new BookingDto
            {
                BookingId = b.BookingId,
                TourId = b.TourId,
                TourName = b.Tour.Name,
                TourPrice = b.Tour.Price,
                TourDurationDays = b.Tour.DurationDays,
                UserName = b.Credential?.UserDetail?.Name,
                UserEmail = b.Credential?.Email,
                UserPhone = b.Credential?.UserDetail?.Phone,
                UserPassportData = b.Credential?.UserDetail?.PassportData,
                UserDateOfBirth = b.Credential?.UserDetail?.DateOfBirth,
                StartDate = b.StartDate,
                BookingDate = b.BookingDate.HasValue
                    ? DateOnly.FromDateTime(b.BookingDate.Value)
                    : null,
                Status = b.Status,
                NumberOfPeople = b.NumberOfPeople,
                TotalPrice = b.TotalPrice,
                Comment = b.Comment,
                Persons = b.BookingPeople.Select(p => new BookingPersonDto
                {
                    Name = p.Name,
                    PassportData = p.PassportData,
                    DateOfBirth = p.DateOfBirth,
                    IsChild = p.IsChild
                }).ToList()
            };
        }

        public decimal Cancel(int credentialId, int bookingId)
        {
            var booking = _repo.GetById(bookingId);

            if (booking == null)
                throw new AppException(
                    "NOT_FOUND",
                    "Бронювання не знайдено"
                );

            if (booking.CredentialId != credentialId)
                throw new AppException(
                    "FORBIDDEN",
                    "Це не ваше бронювання"
                );

            if (booking.Status == BookingStatuses.Cancelled)
                throw new AppException(
                    "ALREADY_CANCELLED",
                    "Бронювання вже скасовано"
                );

            var today = DateOnly.FromDateTime(DateTime.Now);

            if (booking.StartDate < today)
                throw new AppException(
                    "INVALID_CANCELLATION",
                    "Неможливо скасувати бронювання після початку туру"
                );

            var daysUntilStart =
                booking.StartDate.DayNumber - today.DayNumber;

            decimal penaltyAmount = 0;

            if (daysUntilStart < 3)
            {
                penaltyAmount = booking.TotalPrice * 0.30m;

                var penalty = new Penalty
                {
                    BookingId = booking.BookingId,
                    Amount = penaltyAmount,
                    IsPaid = false
                };

                _context.Penalties.Add(penalty);

                _logger.LogInformation(
                    "Нараховано штраф для бронювання ID={BookingId}: {PenaltyAmount}",
                    bookingId,
                    penaltyAmount
                );
            }

            booking.Status = BookingStatuses.Cancelled;

            _context.SaveChanges();

            _logger.LogInformation(
                "Бронювання ID={BookingId} скасовано користувачем ID={UserId}",
                bookingId,
                credentialId
            );

            return penaltyAmount;
        }
    }
}
