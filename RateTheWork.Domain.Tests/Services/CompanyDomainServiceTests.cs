using FluentAssertions;
using Moq;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Interfaces.Services;
using RateTheWork.Domain.Services;
using RateTheWork.Domain.Tests.TestHelpers;
using RateTheWork.Domain.ValueObjects;

namespace RateTheWork.Domain.Tests.Services;

public class CompanyDomainServiceTests : DomainTestBase
{
    private readonly Mock<ICompanyRepository> _companyRepositoryMock;
    private readonly Mock<ICompanyBranchRepository> _branchRepositoryMock;
    private readonly Mock<ITaxNumberValidator> _taxNumberValidatorMock;
    private readonly CompanyDomainService _service;

    public CompanyDomainServiceTests()
    {
        _companyRepositoryMock = CreateMock<ICompanyRepository>();
        _branchRepositoryMock = CreateMock<ICompanyBranchRepository>();
        _taxNumberValidatorMock = CreateMock<ITaxNumberValidator>();

        _service = new CompanyDomainService(
            _companyRepositoryMock.Object,
            _branchRepositoryMock.Object,
            _taxNumberValidatorMock.Object);
    }

    [Fact]
    public async Task CreateCompanyAsync_WithValidData_ShouldCreateCompany()
    {
        // Arrange
        var companyData = new CreateCompanyData
        {
            Name = "Test Company",
            TaxNumber = "1234567890",
            MersisNo = "0123456789012345",
            Type = CompanyType.LimitedCompany,
            Sector = CompanySector.Technology,
            Address = new Address("Turkey", "Istanbul", "Kadikoy", "Tech Street", "34710"),
            PhoneNumber = "+905551234567",
            Email = "info@testcompany.com",
            Website = "https://testcompany.com"
        };

        _companyRepositoryMock.Setup(x => x.GetByTaxNumberAsync(companyData.TaxNumber))
            .ReturnsAsync((Company)null);
        _companyRepositoryMock.Setup(x => x.GetByMersisNoAsync(companyData.MersisNo))
            .ReturnsAsync((Company)null);
        _taxNumberValidatorMock.Setup(x => x.ValidateTaxNumberAsync(companyData.TaxNumber))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateCompanyAsync(companyData);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(companyData.Name);
        result.TaxNumber.Should().Be(companyData.TaxNumber);
        result.MersisNo.Should().Be(companyData.MersisNo);
        result.Type.Should().Be(companyData.Type);
        result.Sector.Should().Be(companyData.Sector);
        result.IsApproved.Should().BeFalse();
        result.ApprovalStatus.Should().Be(ApprovalStatus.Pending);
        _companyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Company>()), Times.Once);
    }

    [Fact]
    public async Task CreateCompanyAsync_WithExistingTaxNumber_ShouldThrowException()
    {
        // Arrange
        var companyData = new CreateCompanyData
        {
            Name = "Test Company",
            TaxNumber = "1234567890",
            MersisNo = "0123456789012345"
        };

        var existingCompany = new Company { TaxNumber = companyData.TaxNumber };
        _companyRepositoryMock.Setup(x => x.GetByTaxNumberAsync(companyData.TaxNumber))
            .ReturnsAsync(existingCompany);

        // Act
        var act = async () => await _service.CreateCompanyAsync(companyData);

        // Assert
        await act.Should().ThrowAsync<DuplicateEntityException>()
            .WithMessage("Company with this tax number already exists");
    }

    [Fact]
    public async Task CreateCompanyAsync_WithExistingMersisNo_ShouldThrowException()
    {
        // Arrange
        var companyData = new CreateCompanyData
        {
            Name = "Test Company",
            TaxNumber = "1234567890",
            MersisNo = "0123456789012345"
        };

        _companyRepositoryMock.Setup(x => x.GetByTaxNumberAsync(companyData.TaxNumber))
            .ReturnsAsync((Company)null);
        
        var existingCompany = new Company { MersisNo = companyData.MersisNo };
        _companyRepositoryMock.Setup(x => x.GetByMersisNoAsync(companyData.MersisNo))
            .ReturnsAsync(existingCompany);

        // Act
        var act = async () => await _service.CreateCompanyAsync(companyData);

        // Assert
        await act.Should().ThrowAsync<DuplicateEntityException>()
            .WithMessage("Company with this MERSIS number already exists");
    }

    [Fact]
    public async Task CreateCompanyAsync_WithInvalidTaxNumber_ShouldThrowException()
    {
        // Arrange
        var companyData = new CreateCompanyData
        {
            Name = "Test Company",
            TaxNumber = "invalid",
            MersisNo = "0123456789012345"
        };

        _companyRepositoryMock.Setup(x => x.GetByTaxNumberAsync(companyData.TaxNumber))
            .ReturnsAsync((Company)null);
        _companyRepositoryMock.Setup(x => x.GetByMersisNoAsync(companyData.MersisNo))
            .ReturnsAsync((Company)null);
        _taxNumberValidatorMock.Setup(x => x.ValidateTaxNumberAsync(companyData.TaxNumber))
            .ReturnsAsync(false);

        // Act
        var act = async () => await _service.CreateCompanyAsync(companyData);

        // Assert
        await act.Should().ThrowAsync<InvalidTaxNumberException>()
            .WithMessage("Invalid tax number");
    }

    [Fact]
    public async Task ApproveCompanyAsync_WithPendingCompany_ShouldApprove()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var approvedBy = "admin@ratethework.com";
        var company = new Company
        {
            Id = companyId,
            Name = "Test Company",
            ApprovalStatus = ApprovalStatus.Pending,
            IsApproved = false
        };

        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(company);

        // Act
        await _service.ApproveCompanyAsync(companyId, approvedBy);

        // Assert
        company.IsApproved.Should().BeTrue();
        company.ApprovalStatus.Should().Be(ApprovalStatus.Approved);
        company.ApprovedBy.Should().Be(approvedBy);
        company.ApprovedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        _companyRepositoryMock.Verify(x => x.UpdateAsync(company), Times.Once);
    }

    [Fact]
    public async Task ApproveCompanyAsync_WithAlreadyApprovedCompany_ShouldNotUpdate()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            Name = "Test Company",
            ApprovalStatus = ApprovalStatus.Approved,
            IsApproved = true,
            ApprovedBy = "previous@admin.com",
            ApprovedAt = DateTime.UtcNow.AddDays(-5)
        };

        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(company);

        // Act
        await _service.ApproveCompanyAsync(companyId, "new@admin.com");

        // Assert
        company.ApprovedBy.Should().Be("previous@admin.com");
        _companyRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Company>()), Times.Never);
    }

    [Fact]
    public async Task RejectCompanyAsync_WithPendingCompany_ShouldReject()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var rejectedBy = "admin@ratethework.com";
        var reason = "Invalid documentation";
        var company = new Company
        {
            Id = companyId,
            Name = "Test Company",
            ApprovalStatus = ApprovalStatus.Pending,
            IsApproved = false
        };

        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(company);

        // Act
        await _service.RejectCompanyAsync(companyId, rejectedBy, reason);

        // Assert
        company.IsApproved.Should().BeFalse();
        company.ApprovalStatus.Should().Be(ApprovalStatus.Rejected);
        company.RejectedBy.Should().Be(rejectedBy);
        company.RejectionReason.Should().Be(reason);
        company.RejectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        _companyRepositoryMock.Verify(x => x.UpdateAsync(company), Times.Once);
    }

    [Fact]
    public async Task AddBranchAsync_WithValidData_ShouldAddBranch()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            Name = "Test Company",
            IsApproved = true,
            ApprovalStatus = ApprovalStatus.Approved
        };

        var branchData = new CreateBranchData
        {
            Name = "Istanbul Branch",
            Address = new Address("Turkey", "Istanbul", "Besiktas", "Branch Street", "34710"),
            PhoneNumber = "+905559876543",
            Email = "istanbul@testcompany.com",
            IsHeadquarters = false
        };

        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(company);

        // Act
        var result = await _service.AddBranchAsync(companyId, branchData);

        // Assert
        result.Should().NotBeNull();
        result.CompanyId.Should().Be(companyId);
        result.Name.Should().Be(branchData.Name);
        result.IsHeadquarters.Should().Be(branchData.IsHeadquarters);
        _branchRepositoryMock.Verify(x => x.AddAsync(It.IsAny<CompanyBranch>()), Times.Once);
    }

    [Fact]
    public async Task AddBranchAsync_WithUnapprovedCompany_ShouldThrowException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            Name = "Test Company",
            IsApproved = false,
            ApprovalStatus = ApprovalStatus.Pending
        };

        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(company);

        // Act
        var act = async () => await _service.AddBranchAsync(companyId, new CreateBranchData());

        // Assert
        await act.Should().ThrowAsync<CompanyNotActiveException>()
            .WithMessage("Cannot add branch to unapproved company");
    }

    [Fact]
    public async Task GetCompanyBranchesAsync_ShouldReturnBranches()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var branches = new List<CompanyBranch>
        {
            new CompanyBranch { CompanyId = companyId, Name = "Branch 1" },
            new CompanyBranch { CompanyId = companyId, Name = "Branch 2" }
        };

        _branchRepositoryMock.Setup(x => x.GetByCompanyIdAsync(companyId))
            .ReturnsAsync(branches);

        // Act
        var result = await _service.GetCompanyBranchesAsync(companyId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(branches);
    }

    [Fact]
    public async Task UpdateCompanyAsync_WithValidData_ShouldUpdate()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            Name = "Old Name",
            Sector = CompanySector.Technology,
            IsApproved = true
        };

        var updateData = new UpdateCompanyData
        {
            Name = "New Name",
            Sector = CompanySector.Finance,
            PhoneNumber = "+905551234567",
            Email = "new@company.com",
            Website = "https://newcompany.com"
        };

        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(company);

        // Act
        await _service.UpdateCompanyAsync(companyId, updateData);

        // Assert
        company.Name.Should().Be(updateData.Name);
        company.Sector.Should().Be(updateData.Sector);
        company.PhoneNumber.Should().Be(updateData.PhoneNumber);
        company.Email.Should().Be(updateData.Email);
        company.Website.Should().Be(updateData.Website);
        _companyRepositoryMock.Verify(x => x.UpdateAsync(company), Times.Once);
    }

    [Fact]
    public async Task GetCompanyStatisticsAsync_ShouldReturnStatistics()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            Name = "Test Company",
            TotalReviews = 150,
            AverageRating = 4.2
        };

        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(company);

        // Act
        var result = await _service.GetCompanyStatisticsAsync(companyId);

        // Assert
        result.Should().NotBeNull();
        result.TotalReviews.Should().Be(150);
        result.AverageRating.Should().Be(4.2);
    }

    [Fact]
    public async Task IsCompanyActiveAsync_WithApprovedCompany_ShouldReturnTrue()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            IsApproved = true,
            ApprovalStatus = ApprovalStatus.Approved
        };

        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(company);

        // Act
        var result = await _service.IsCompanyActiveAsync(companyId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCompanyActiveAsync_WithRejectedCompany_ShouldReturnFalse()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            IsApproved = false,
            ApprovalStatus = ApprovalStatus.Rejected
        };

        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(company);

        // Act
        var result = await _service.IsCompanyActiveAsync(companyId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsCompanyActiveAsync_WithNonExistentCompany_ShouldReturnFalse()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync((Company)null);

        // Act
        var result = await _service.IsCompanyActiveAsync(companyId);

        // Assert
        result.Should().BeFalse();
    }
}