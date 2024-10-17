namespace DTOs
{
    public class RefreshTokenDto
    {
        public string? Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
