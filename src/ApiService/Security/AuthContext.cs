using ApiService.Entities;

namespace ApiService.Security;

public class AuthContext
{
    public Users? Actor { get; set; }

    public bool IsAuthenticated => Actor is not null;
}
