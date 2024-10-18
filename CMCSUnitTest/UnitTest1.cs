using CMCS.Controllers;
using CMCS.Data;
using CMCS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CMCSUnitTest
{
    public class ClaimsControllerTests
    {
        private readonly ClaimsController _controller;
        private readonly Mock<ApplicationDbContext> _dbContextMock;
        private readonly Mock<DbSet<Claim>> _claimsDbSetMock;

        public ClaimsControllerTests()
        {
            // Mocking ApplicationDbContext and DbSet<Claim>
            _dbContextMock = new Mock<ApplicationDbContext>();

            var claims = new List<Claim>
            {
                new Claim { ClaimId = 1, LecturerName = "John Doe", HoursWorked = 10, HourlyRate = 20m, Status = "Pending", SubmissionDate = DateTime.Now, DocumentPath = "/uploads/doc1.pdf" },
                new Claim { ClaimId = 2, LecturerName = "Jane Doe", HoursWorked = 12, HourlyRate = 22m, Status = "Pending", SubmissionDate = DateTime.Now, DocumentPath = "/uploads/doc2.pdf" }
            };

            // Use CreateDbSetMock extension method to mock the DbSet
            _claimsDbSetMock = claims.CreateDbSetMock();
            _dbContextMock.Setup(m => m.Claims).Returns(_claimsDbSetMock.Object);
            _dbContextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            _controller = new ClaimsController(_dbContextMock.Object);
        }

        [Fact]
        public async Task SubmitClaim_ValidClaim_SavesClaimAndRedirects()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "John Doe",
                HoursWorked = 10,
                HourlyRate = 20m,
                SubmissionDate = DateTime.Now,
                DocumentPath = "/uploads/test.pdf"
            };

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SubmitClaim(claim, fileMock.Object);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            _claimsDbSetMock.Verify(db => db.Add(It.IsAny<Claim>()), Times.Once);
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ApproveClaim_ValidClaimId_UpdatesClaimStatusAndSaves()
        {
            // Arrange
            var claimId = 1;
            var claim = new Claim { ClaimId = claimId, Status = "Pending" };
            _claimsDbSetMock.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(claim);

            // Act
            var result = await _controller.ApproveClaim(claimId);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Approved", claim.Status); // Verify status updated
            _dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); // Verify SaveChangesAsync was called
        }

        [Fact]
        public async Task RejectClaim_ValidClaimId_UpdatesClaimStatusAndSaves()
        {
            // Arrange
            var claimId = 2;
            var claim = new Claim { ClaimId = claimId, Status = "Pending" };
            _claimsDbSetMock.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(claim);

            // Act
            var result = await _controller.RejectClaim(claimId);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Rejected", claim.Status); // Verify status updated
            _dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); // Verify SaveChangesAsync was called
        }
    }

    // Extension method to mock DbSet<T>
    public static class DbSetMockExtensions
    {
        public static Mock<DbSet<T>> CreateDbSetMock<T>(this IEnumerable<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();

            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            dbSetMock.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Append(s));

            return dbSetMock;
        }
    }
}
