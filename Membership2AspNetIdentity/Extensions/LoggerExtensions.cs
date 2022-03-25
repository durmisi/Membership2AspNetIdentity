using Microsoft.AspNetCore.Identity;

using Serilog.Core;

namespace Membership2AspNetIdentity.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogIdentityResult(this Logger logger, IdentityResult? result, MembershipContext.User membershipUser)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (membershipUser is null)
            {
                throw new ArgumentNullException(nameof(membershipUser));
            }

            if (result != null)
            {
                foreach (var error in result.Errors)
                {
                    logger.Error($"ID = {membershipUser.UserId}, UserName={membershipUser.UserName}, Description={error.Description}");
                }
            }
        }
    }
}
