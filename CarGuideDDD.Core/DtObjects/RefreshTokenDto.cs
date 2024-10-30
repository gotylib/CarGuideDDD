namespace CarGuideDDD.Core.DtObjects
{
    public class RefreshTokenDto
    {
        public string? Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
