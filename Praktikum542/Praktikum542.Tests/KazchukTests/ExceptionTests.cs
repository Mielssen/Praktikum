using System;
using Xunit;
using FluentAssertions;
using Praktikum542.Exceptions;

namespace Praktikum542.Tests.KazchukTests
{
    public class ExceptionTests
    {
        [Fact]
        public void AppException_WithCodeAndMessage_SetsPropertiesCorrectly()
        {
            var exception = new AppException("INVALID_EMAIL", "Некоректний формат email");

            exception.Code.Should().Be("INVALID_EMAIL");
            exception.Message.Should().Be("Некоректний формат email");
        }

        [Fact]
        public void AppException_WhenThrown_CanBeCaughtWithExpectedCode()
        {
            Action act = () => throw new AppException("UNDERAGE", "Користувач має бути повнолітнім");

            act.Should().Throw<AppException>()
               .Where(ex => ex.Code == "UNDERAGE");
        }

        [Fact]
        public void AppException_NotFound_ThrowsExpectedException()
        {
            Action act = () => throw new AppException("NOT_FOUND", "Тур або бронювання не знайдено");

            act.Should().Throw<AppException>()
               .Where(ex => ex.Code == "NOT_FOUND");
        }

        [Fact]
        public void AppException_InvalidPassword_ThrowsExpectedException()
        {
            Action act = () => throw new AppException("INVALID_PASSWORD", "Пароль занадто короткий");

            act.Should().Throw<AppException>()
               .Where(ex => ex.Code == "INVALID_PASSWORD");
        }

        [Fact]
        public void AppException_AccountDisabled_ThrowsExpectedException()
        {
            Action act = () => throw new AppException("ACCOUNT_DISABLED", "Акаунт деактивовано");

            act.Should().Throw<AppException>()
               .Where(ex => ex.Code == "ACCOUNT_DISABLED");
        }
    }
}