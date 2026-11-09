using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Please enter your email."),
     EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Please enter your password."), DataType(DataType.Password)]
    public string Password { get; set; } = "";
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Please enter your full name."), StringLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Please enter your email."),
     EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Please choose a password."),
     MinLength(8, ErrorMessage = "Password must be at least 8 characters."),
     DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Compare("Password", ErrorMessage = "Passwords do not match."), DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = "";

    [Required(ErrorMessage = "Please choose a role.")]
    public string Role { get; set; } = "";
}

public record LoginResponse(string Message, string Role, int UserId, string FullName);