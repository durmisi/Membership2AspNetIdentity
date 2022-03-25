using Membership2AspNetIdentity.AspNetIdentity;
using Membership2AspNetIdentity.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

var services = new ServiceCollection();
services.AddLogging(logging => {

    logging.ClearProviders();
    logging.AddConsole();
});

var builder = new ConfigurationBuilder()
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json");

var configuration = builder.Build();

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

var workflowConfiguration = MigrationWorkflowConfiguration.FromConfiguration(configuration);

services.AddSingleton<IConfiguration>(configuration);

services.AddIdentityEx(configuration);

services.AddSingleton<MembershipContext>();
services.AddHttpClient<Membership2AspNetIdentityApiClient>();

using (var provider = services.BuildServiceProvider())
{
    logger.Information("Migration is starting....");

    // Migrate Identity database
    logger.Information("Step1: Creating the AspNetIdentity database...");

    var context = provider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    UserManager<ApplicationUser> _userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

    logger.Information("Step2: Reading the Users from Membership database...");

    var apiClient = provider.GetRequiredService<Membership2AspNetIdentityApiClient>();

    MembershipContext membeshipContext = provider.GetRequiredService<MembershipContext>();
    IEnumerable<MembershipContext.User> membershipUsers = await membeshipContext.GetUsers();
    IEnumerable<MembershipContext.Membership> memberships = await membeshipContext.GetMemberships();

    logger.Information("Step3: Migration of Membership users...");
    logger.Information($"Step3: Total {membershipUsers.Count()} found in the database...");

    foreach (MembershipContext.User membershipUser in membershipUsers)
    {
        MembershipContext.Membership? membershipRecord = memberships
            .Where(x => x.UserId == membershipUser.UserId)
            .Where(x => x.ApplicationId == membershipUser.ApplicationId)
            .FirstOrDefault();

        if (membershipRecord == null)
        {
            continue;
        }

        if (string.IsNullOrEmpty(membershipRecord.Email) && workflowConfiguration.IgnoreUserIfEmailIsEmpty)
        {
            logger.Information($"The user with UserName = {membershipUser.UserName} is ignored. Email address is empty.");
            continue;
        }

        bool isLockedOut = false;
        string? decryptedPassword = null;
        try
        {
            //Get password from API
            var membershipPasswordResult = await apiClient.GetClearTextPassword(membershipUser.UserName);

            if (membershipPasswordResult == null || membershipPasswordResult.Pex)
            {
                if (membershipPasswordResult != null && membershipPasswordResult.Pex)
                {
                    logger.Error(@$"Unable to retrieve the password for user with UserName={membershipUser.UserName} 
                        a Pex error ocured. {membershipPasswordResult.PexMessage}"
                    );
                }
                else
                {
                    logger.Error(@$"Unable to retrieve the password for user with UserName={membershipUser.UserName}");
                }
            }
            else
            {
                logger.Information($"The password for user with UserName = {membershipUser.UserName} is {membershipPasswordResult.Password}");
            }

            decryptedPassword = membershipPasswordResult?.Password;

            isLockedOut = membershipPasswordResult != null
                ? membershipPasswordResult.IsLockedOut
                : false;

        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Unable to retrieve the password for user with UserName={membershipUser.UserName}");
        }

        if (string.IsNullOrEmpty(decryptedPassword))
        {
            if (workflowConfiguration.IgnoreUserIfPasswordIsEmpty)
            {
                continue;
            }

            if (workflowConfiguration.GeneratePassword)
            {
                decryptedPassword = new PasswordGenerator()
                                            .Generate(_userManager.Options);
            }
            else
            {
                decryptedPassword = workflowConfiguration.DefaultPassword;
            }
        }

        var aspNetIdentityUser = await _userManager.FindByNameAsync(membershipUser.UserName);
        if (aspNetIdentityUser == null)
        {
            //NEW USER
            var createUserResult = await _userManager.CreateAsync(new ApplicationUser()
            {
                Id = membershipUser.UserId,
                UserName = membershipUser.UserName,
                Email = membershipRecord.Email,
            });

            if (createUserResult == null || !createUserResult.Succeeded)
            {
                logger.Error("Unable to migrate the user with ID={0} and UserName={1}", membershipUser.UserId, membershipUser.UserName);
                logger.LogIdentityResult(createUserResult, membershipUser);
                continue;
            }

            // read the user again
            aspNetIdentityUser = await _userManager.FindByNameAsync(membershipUser.UserName);
        }
        else
        {
            //EXISTING USER
            aspNetIdentityUser.Email = membershipRecord.Email;

            //aspNetIdentityUser.CreateDate = membershipRecord.CreateDate;
            //aspNetIdentityUser.LastLoginDate = membershipRecord.LastLoginDate;

            //aspNetIdentityUser.LastPasswordChangedDate = membershipRecord.LastPasswordChangedDate;
            //aspNetIdentityUser.LastLockoutDate = membershipRecord.LastLockoutDate;
            //aspNetIdentityUser.LastActivityDate = membershipUser.LastActivityDate;

            var updateUserResult = await _userManager.UpdateAsync(aspNetIdentityUser);

            if (updateUserResult == null || !updateUserResult.Succeeded)
            {
                logger.Error("Unable to update the user with ID={0} and UserName={1}", membershipUser.UserId, membershipUser.UserName);
                logger.LogIdentityResult(updateUserResult, membershipUser);
            }
        }

        var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(aspNetIdentityUser);
        var resetPasswordResult = await _userManager.ResetPasswordAsync(aspNetIdentityUser, resetPasswordToken, decryptedPassword);

        if (resetPasswordResult == null || !resetPasswordResult.Succeeded)
        {
            logger.Error("Unable to set the password for user with ID={0} and UserName={1}", membershipUser.UserId, membershipUser.UserName);
            logger.LogIdentityResult(resetPasswordResult, membershipUser);
        }

        if (isLockedOut)
        {
            //The user was locked out in Membership database, so we should lock the user in the asp.net identity database as well
            var setLockoutResult = await _userManager.SetLockoutEnabledAsync(aspNetIdentityUser, true);

            if (setLockoutResult == null || !setLockoutResult.Succeeded)
            {
                logger.Error("Unable to set the lockout for user with ID={0} and UserName={1}", membershipUser.UserId, membershipUser.UserName);
                logger.LogIdentityResult(setLockoutResult, membershipUser);
            }
        }

    }

}
