namespace MagicVilla_API.Models.Dto
{
    public class LoginResponseDTO
    {
        public UserDTO User { get; set; }
        public string Token { get; set; }

        //for Jwt
        //public LocalUser User { get; set; }

        //when use Identity we get role form the token here returned null & exception .
        //public string Role { get; set; }
    }
}
