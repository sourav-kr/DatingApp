using System.ComponentModel.DataAnnotations;

namespace DatingApp.Api.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string userName { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 4, ErrorMessage = "Password should be between 4 and 10")]
        public string password { get; set; }

    }
}