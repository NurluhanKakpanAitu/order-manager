namespace Domain.Entities.Base;

public sealed class Translation
{
    public string? Kz { get; set; }
    public string? Ru { get; set; }
    public string? En { get; set; }

    public static Translation Create(string? kz = null, string? ru = null, string? en = null)
    {
        if (string.IsNullOrWhiteSpace(kz) && string.IsNullOrWhiteSpace(ru) && string.IsNullOrWhiteSpace(en))
            throw new ArgumentException("At least one translation must be provided.");

        return new Translation { Kz = kz, Ru = ru, En = en };
    }
    
}