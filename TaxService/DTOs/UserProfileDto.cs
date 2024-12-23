namespace TaxService.DTOs;

public class UserProfileDto {
    public int UserId { get; set; }
    public string? Email { get; set; } 
    
    public string? FullName { get; set; }
}