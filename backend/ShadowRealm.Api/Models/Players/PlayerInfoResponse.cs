namespace ShadowRealm.Api.Models.Responses;

public class PlayerInfoResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Experience { get; set; }
}