using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Concertify.Application.Services;

public class CustomTotpTokenProvider<TUser> : TotpSecurityStampBasedTokenProvider<TUser> where TUser : class
{
    public override async Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
    {
        var isTwoFactorEnabled = await manager.GetTwoFactorEnabledAsync(user);
        if (!isTwoFactorEnabled)
            return false;
        
        var securityStamp = await manager.GetSecurityStampAsync(user);
        return !string.IsNullOrEmpty(securityStamp);
    }

}
