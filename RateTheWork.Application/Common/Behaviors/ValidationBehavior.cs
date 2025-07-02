using FluentValidation;
using MediatR;

namespace RateTheWork.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior - Tüm request'leri FluentValidation ile doğrular.
/// Handler çalışmadan önce validation kurallarını kontrol eder.
/// </summary>
/// <typeparam name="TRequest">MediatR request tipi</typeparam>
/// <typeparam name="TResponse">MediatR response tipi</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// DI container'dan gelen tüm validator'lar.
    /// Örneğin RegisterUserCommand için RegisterUserCommandValidator otomatik inject edilir.
    /// </summary>
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// ValidationBehavior constructor
    /// </summary>
    /// <param name="validators">İlgili request için tanımlanmış tüm validator'lar</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// Pipeline'daki bir sonraki behavior'u çağırmadan önce validation yapar
    /// </summary>
    /// <param name="request">Validate edilecek request</param>
    /// <param name="next">Pipeline'daki bir sonraki behavior veya handler</param>
    /// <param name="cancellationToken">İşlem iptali için token</param>
    /// <returns>Handler'dan dönen response</returns>
    /// <exception cref="ValidationException">Validation hataları varsa fırlatılır</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Eğer bu request için hiç validator yoksa, validation'ı atla
        if (_validators.Any())
        {
            // FluentValidation context'i oluştur
            var context = new ValidationContext<TRequest>(request);

            // Tüm validator'ları paralel olarak çalıştır
            // Örnek: RegisterUserCommand için hem RegisterUserCommandValidator
            // hem de başka validator varsa hepsi aynı anda çalışır
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            // Tüm validation sonuçlarından hataları topla
            // SelectMany ile nested error listelerini düzleştir (flatten)
            var failures = validationResults
                .Where(r => r.Errors.Any())        // Hatası olan sonuçları filtrele
                .SelectMany(r => r.Errors)         // Tüm hataları tek liste haline getir
                .ToList();

            // Eğer herhangi bir validation hatası varsa exception fırlat
            if (failures.Any())
            {
                // Bu exception WebAPI'de otomatik olarak 400 Bad Request'e dönüşür
                throw new ValidationException(failures);
            }
        }

        // Validation başarılı, bir sonraki behavior'a veya handler'a devam et
        return await next();
    }
}