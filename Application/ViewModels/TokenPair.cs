namespace Application.ViewModels
{
    public class TokenPair
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenValidDate { get; set; }
    }
}
