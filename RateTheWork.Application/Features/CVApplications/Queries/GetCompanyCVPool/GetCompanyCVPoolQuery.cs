using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Application.Common.Models;
using RateTheWork.Domain.Enums.CVApplication;
using RateTheWork.Domain.Interfaces;
using RateTheWork.Domain.Entities;

namespace RateTheWork.Application.Features.CVApplications.Queries.GetCompanyCVPool;

/// <summary>
/// Şirketin CV havuzunu getirme sorgusu
/// </summary>
public record GetCompanyCVPoolQuery : IRequest<GetCompanyCVPoolResult>
{
    /// <summary>
    /// Şirket ID'si (boş ise kullanıcının şirketi)
    /// </summary>
    public string? CompanyId { get; init; }
    
    /// <summary>
    /// Departman ID'si (filtreleme için)
    /// </summary>
    public string? DepartmentId { get; init; }
    
    /// <summary>
    /// Durum filtresi
    /// </summary>
    public CVApplicationStatus? Status { get; init; }
    
    /// <summary>
    /// Tarih aralığı - başlangıç
    /// </summary>
    public DateTime? StartDate { get; init; }
    
    /// <summary>
    /// Tarih aralığı - bitiş
    /// </summary>
    public DateTime? EndDate { get; init; }
    
    /// <summary>
    /// Sayfa numarası
    /// </summary>
    public int Page { get; init; } = 1;
    
    /// <summary>
    /// Sayfa boyutu
    /// </summary>
    public int PageSize { get; init; } = 20;
    
    /// <summary>
    /// Sıralama
    /// </summary>
    public string OrderBy { get; init; } = "SubmittedAt";
    
    /// <summary>
    /// Sıralama yönü
    /// </summary>
    public bool IsDescending { get; init; } = true;
}

/// <summary>
/// CV havuzu sonucu
/// </summary>
public record GetCompanyCVPoolResult
{
    public PaginatedList<CVApplicationDto> Applications { get; init; } = null!;
    public CVPoolStatistics Statistics { get; init; } = null!;
}

/// <summary>
/// CV başvuru DTO
/// </summary>
public record CVApplicationDto
{
    public string Id { get; init; } = string.Empty;
    public string ApplicantName { get; init; } = string.Empty;
    public string ApplicantEmail { get; init; } = string.Empty;
    public string ApplicantPhone { get; init; } = string.Empty;
    public string? ApplicantWebsite { get; init; }
    public List<DepartmentDto> Departments { get; init; } = new();
    public CVApplicationStatus Status { get; init; }
    public DateTime SubmittedAt { get; init; }
    public DateTime? ViewedAt { get; init; }
    public DateTime? DownloadedAt { get; init; }
    public DateTime? RespondedAt { get; init; }
    public string? ResponseMessage { get; init; }
    public DateTime ExpiryDate { get; init; }
    public DateTime? FeedbackDeadline { get; init; }
    public bool IsFeedbackOverdue { get; init; }
    public int DaysUntilExpiry { get; init; }
    public string CVFileUrl { get; init; } = string.Empty;
    public string? MotivationLetterUrl { get; init; }
    public bool HasMotivationLetter => !string.IsNullOrEmpty(MotivationLetterUrl);
    public Dictionary<string, string> AdditionalInfo { get; init; } = new();
}

/// <summary>
/// Departman DTO
/// </summary>
public record DepartmentDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

/// <summary>
/// CV havuzu istatistikleri
/// </summary>
public record CVPoolStatistics
{
    public int TotalApplications { get; init; }
    public int NewApplications { get; init; }
    public int ViewedApplications { get; init; }
    public int DownloadedApplications { get; init; }
    public int RespondedApplications { get; init; }
    public int ExpiredApplications { get; init; }
    public int FeedbackOverdueCount { get; init; }
    public Dictionary<string, int> ApplicationsByDepartment { get; init; } = new();
    public Dictionary<CVApplicationStatus, int> ApplicationsByStatus { get; init; } = new();
}

/// <summary>
/// CV havuzu query handler
/// </summary>
public class GetCompanyCVPoolQueryHandler : IRequestHandler<GetCompanyCVPoolQuery, GetCompanyCVPoolResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetCompanyCVPoolQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<GetCompanyCVPoolResult> Handle(GetCompanyCVPoolQuery request, CancellationToken cancellationToken)
    {
        // 1. Yetki kontrolü
        var user = await _unitOfWork.Users.GetByIdAsync(_currentUserService.UserId!);
        if (user == null)
        {
            throw new UnauthorizedException("Bu işlem için yetkiniz yok.");
        }

        var hasCompanyRole = user.UserRoles.Any(role => 
            role == "CompanyAdmin" || 
            role == "CompanyHR" || 
            role == "CompanyManager");

        if (!hasCompanyRole)
        {
            throw new ForbiddenAccessException("CV havuzunu görüntülemek için şirket yetkilisi olmalısınız.");
        }

        // 2. Şirket ID'sini belirle
        var companyId = request.CompanyId;
        if (string.IsNullOrEmpty(companyId))
        {
            // TODO: Kullanıcının şirket ID'sini al
            throw new BusinessRuleException("COMPANY_NOT_FOUND", "Şirket bilgisi bulunamadı.");
        }

        // 3. CV başvurularını getir
        var applications = await _unitOfWork.CVApplications.GetByCompanyIdAsync(
            companyId, 
            request.Page, 
            request.PageSize);

        // 4. Filtreleme uygula
        if (!string.IsNullOrEmpty(request.DepartmentId))
        {
            applications = applications.Where(a => a.DepartmentIds.Contains(request.DepartmentId)).ToList();
        }

        if (request.Status.HasValue)
        {
            applications = applications.Where(a => a.Status == request.Status.Value).ToList();
        }

        if (request.StartDate.HasValue)
        {
            applications = applications.Where(a => a.SubmittedAt >= request.StartDate.Value).ToList();
        }

        if (request.EndDate.HasValue)
        {
            applications = applications.Where(a => a.SubmittedAt <= request.EndDate.Value).ToList();
        }

        // 5. Sıralama
        applications = request.OrderBy.ToLower() switch
        {
            "submittedat" => request.IsDescending 
                ? applications.OrderByDescending(a => a.SubmittedAt).ToList()
                : applications.OrderBy(a => a.SubmittedAt).ToList(),
            "applicantname" => request.IsDescending 
                ? applications.OrderByDescending(a => a.ApplicantName).ToList()
                : applications.OrderBy(a => a.ApplicantName).ToList(),
            "status" => request.IsDescending 
                ? applications.OrderByDescending(a => a.Status).ToList()
                : applications.OrderBy(a => a.Status).ToList(),
            _ => applications.OrderByDescending(a => a.SubmittedAt).ToList()
        };

        // 6. Departman bilgilerini yükle
        var departmentIds = applications.SelectMany(a => a.DepartmentIds).Distinct().ToList();
        var departments = new Dictionary<string, Department>();
        
        foreach (var deptId in departmentIds)
        {
            var dept = await _unitOfWork.Departments.GetByIdAsync(deptId);
            if (dept != null)
            {
                departments[deptId] = dept;
            }
        }

        // 7. DTO'ya dönüştür
        var applicationDtos = applications.Select(a => new CVApplicationDto
        {
            Id = a.Id,
            ApplicantName = a.ApplicantName,
            ApplicantEmail = a.ApplicantEmail,
            ApplicantPhone = a.ApplicantPhone,
            ApplicantWebsite = a.ApplicantWebsite,
            Departments = a.DepartmentIds
                .Where(id => departments.ContainsKey(id))
                .Select(id => new DepartmentDto 
                { 
                    Id = id, 
                    Name = departments[id].Name 
                })
                .ToList(),
            Status = a.Status,
            SubmittedAt = a.SubmittedAt,
            ViewedAt = a.ViewedAt,
            DownloadedAt = a.DownloadedAt,
            RespondedAt = a.RespondedAt,
            ResponseMessage = a.ResponseMessage,
            ExpiryDate = a.ExpiryDate,
            FeedbackDeadline = a.FeedbackDeadline,
            IsFeedbackOverdue = a.IsFeedbackOverdue(),
            DaysUntilExpiry = (int)(a.ExpiryDate - DateTime.UtcNow).TotalDays,
            CVFileUrl = a.CVFileUrl,
            MotivationLetterUrl = a.MotivationLetterUrl,
            AdditionalInfo = a.AdditionalInfo
        }).ToList();

        // 8. İstatistikleri hesapla
        var statistics = await _unitOfWork.CVApplications.GetStatisticsAsync(companyId);
        
        var cvPoolStats = new CVPoolStatistics
        {
            TotalApplications = statistics.TotalApplications,
            NewApplications = statistics.PendingApplications,
            ViewedApplications = statistics.ViewedApplications,
            DownloadedApplications = statistics.DownloadedApplications,
            RespondedApplications = statistics.RespondedApplications,
            ExpiredApplications = applications.Count(a => a.IsExpired()),
            FeedbackOverdueCount = applications.Count(a => a.IsFeedbackOverdue()),
            ApplicationsByDepartment = statistics.ApplicationsByDepartment,
            ApplicationsByStatus = statistics.ApplicationsByStatus
        };

        // 9. Sayfalama
        var paginatedList = new PaginatedList<CVApplicationDto>(
            applicationDtos,
            statistics.TotalApplications,
            request.Page,
            request.PageSize
        );

        return new GetCompanyCVPoolResult
        {
            Applications = paginatedList,
            Statistics = cvPoolStats
        };
    }
}