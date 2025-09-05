using Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Persistence.Services;

public class LocalizationService(IHttpContextAccessor httpContextAccessor) : ILocalizationService
{
    private const string DefaultLanguageCode = "ru";
    
    public string GetCurrentLanguageCode()
    {
        var httpContext = httpContextAccessor.HttpContext;
        
        if (httpContext == null)
            return DefaultLanguageCode;
        
        if (httpContext.Request.Query.TryGetValue("lang", out var langQuery))
        {
            var language = langQuery.FirstOrDefault();
            if (!string.IsNullOrEmpty(language) && IsValidLanguageCode(language.ToLower()))
                return language.ToLower();
        }

        return DefaultLanguageCode;
    }

    private static bool IsValidLanguageCode(string languageCode)
    {
        return languageCode is "ru" or "kz" or "en";
    }
}