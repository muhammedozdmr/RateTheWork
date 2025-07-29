using System.Globalization;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Infrastructure.Services;

public class TcIdentityValidationService : ITcIdentityValidationService
{
    private const string ServiceUrl = "https://tckimlik.nvi.gov.tr/Service/KPSPublic.asmx";
    private readonly HttpClient _httpClient;
    private readonly ILogger<TcIdentityValidationService> _logger;

    public TcIdentityValidationService
    (
        ILogger<TcIdentityValidationService> logger
        , HttpClient httpClient
    )
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> ValidateAsync
    (
        string tcIdentityNumber
        , string firstName
        , string lastName
        , int birthYear
        , CancellationToken cancellationToken = default
    )
    {
        // First check format
        if (!IsValidFormat(tcIdentityNumber))
        {
            _logger.LogWarning("Invalid TC Identity Number format: {TcIdentityNumber}", tcIdentityNumber);
            return false;
        }

        // Then validate with government service
        return await ValidateWithGovernmentServiceAsync(
            tcIdentityNumber,
            firstName,
            lastName,
            birthYear,
            cancellationToken);
    }

    public bool IsValidFormat(string tcIdentityNumber)
    {
        // TC Identity Number must be 11 digits
        if (string.IsNullOrWhiteSpace(tcIdentityNumber) || tcIdentityNumber.Length != 11)
            return false;

        // Must contain only digits
        if (!tcIdentityNumber.All(char.IsDigit))
            return false;

        // First digit cannot be 0
        if (tcIdentityNumber[0] == '0')
            return false;

        // Algorithm validation
        int[] digits = tcIdentityNumber.Select(c => int.Parse(c.ToString())).ToArray();

        // 10th digit validation
        int sumOdd = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        int sumEven = digits[1] + digits[3] + digits[5] + digits[7];
        int digit10 = ((sumOdd * 7) - sumEven) % 10;

        if (digit10 < 0)
            digit10 += 10;

        if (digits[9] != digit10)
            return false;

        // 11th digit validation
        int sumAll = digits.Take(10).Sum();
        int digit11 = sumAll % 10;

        return digits[10] == digit11;
    }

    public async Task<bool> ValidateWithGovernmentServiceAsync
    (
        string tcIdentityNumber
        , string firstName
        , string lastName
        , int birthYear
        , CancellationToken cancellationToken = default
    )
    {
        try
        {
            var soapEnvelope = BuildSoapEnvelope(tcIdentityNumber, firstName, lastName, birthYear);

            var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "http://tckimlik.nvi.gov.tr/WS/TCKimlikNoDogrula");

            var response = await _httpClient.PostAsync(ServiceUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("TC Identity validation service returned error: {StatusCode}", response.StatusCode);
                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseSoapResponse(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating TC Identity Number");
            // In case of service failure, we might want to allow the user to proceed
            // This is a business decision
            return false;
        }
    }

    private string BuildSoapEnvelope(string tcIdentityNumber, string firstName, string lastName, int birthYear)
    {
        // Turkish characters should be uppercase
        firstName = firstName.ToUpper(new CultureInfo("tr-TR"));
        lastName = lastName.ToUpper(new CultureInfo("tr-TR"));

        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
               xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
               xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <TCKimlikNoDogrula xmlns=""http://tckimlik.nvi.gov.tr/WS"">
            <TCKimlikNo>{tcIdentityNumber}</TCKimlikNo>
            <Ad>{firstName}</Ad>
            <Soyad>{lastName}</Soyad>
            <DogumYili>{birthYear}</DogumYili>
        </TCKimlikNoDogrula>
    </soap:Body>
</soap:Envelope>";
    }

    private bool ParseSoapResponse(string responseContent)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(responseContent);

            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            namespaceManager.AddNamespace("ns", "http://tckimlik.nvi.gov.tr/WS");

            var resultNode = doc.SelectSingleNode("//ns:TCKimlikNoDogrulaResult", namespaceManager);

            if (resultNode != null && bool.TryParse(resultNode.InnerText, out bool result))
            {
                return result;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing SOAP response");
            return false;
        }
    }
}
