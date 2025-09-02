using Banking.Api.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Api.Tests
{
    public class AccountNumberGeneratorTests
    {
        private readonly AccountNumberGenerator _generator = new();

        [Fact]
        public void Generate_ShouldReturnStringWithCorrectLength()
        {
            // Act
            var accountNumber = _generator.Generate();

            // Assert
            Assert.Equal(14, accountNumber.Length);
            // yyyyMMdd (8 chars) + 6 random digits
        }

        [Fact]
        public void Generate_ShouldStartWithCurrentDate()
        {
            // Arrange
            var todayPrefix = DateTime.UtcNow.ToString("yyyyMMdd");

            // Act
            var accountNumber = _generator.Generate();

            // Assert
            Assert.StartsWith(todayPrefix, accountNumber);
        }

        [Fact]
        public void Generate_ShouldProduceDifferentNumbersOnMultipleCalls()
        {
            // Act
            var account1 = _generator.Generate();
            var account2 = _generator.Generate();

            // Assert
            Assert.NotEqual(account1, account2);
        }

        [Fact]
        public void Generate_ShouldEndWithDigitsOnly()
        {
            // Act
            var accountNumber = _generator.Generate();
            var suffix = accountNumber.Substring(8); // last 6 chars

            // Assert
            Assert.True(int.TryParse(suffix, out _));
        }
    }
}
