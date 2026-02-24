using System.ComponentModel.DataAnnotations;

namespace ChatApp.Data;

/// <summary>
/// Represents a customizable banner associated with a specific program (role).
/// </summary>
public class ProgramBanner
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(256)]
    public string RoleName { get; set; } = string.Empty;

    [Required]
    public string BannerText { get; set; } = string.Empty;

    [StringLength(50)]
    public string BackgroundColor { get; set; } = "#007bff";

    [StringLength(50)]
    public string TextColor { get; set; } = "#ffffff";

    public bool IsActive { get; set; } = true;
}
