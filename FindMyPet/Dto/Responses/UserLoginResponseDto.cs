namespace FindMyPet.Dto.Responses
{
    public class UserLoginResponseDto
    {
        public UserDto User { get; set; }
        public string Jwt { get; set; }
        public string RefreshToken { get; set; }
    }
}
