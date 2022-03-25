using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Dapper;

public class MembershipContext
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;
    public MembershipContext(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("Membership");
    }
    public IDbConnection CreateConnection()
        => new SqlConnection(_connectionString);

    public async Task<IEnumerable<User>> GetUsers()
    {
        var query = @"
            SELECT *
              FROM [dbo].[aspnet_Users]
        ";

        using (var connection = CreateConnection())
        {
            var users = await connection.QueryAsync<User>(query);
            return users.ToList();
        }
    }

    public async Task<IEnumerable<Membership>> GetMemberships()
    {
        var query = @"
            SELECT *
            FROM[dbo].[aspnet_Membership]
        ";

        using (var connection = CreateConnection())
        {
            var memberships = await connection.QueryAsync<Membership>(query);
            return memberships.ToList();
        }
    }

    public async Task<Membership> GetMembershipsBy(Guid applicationId, Guid userId)
    {
        var query = @$"
            SELECT *
            FROM[dbo].[aspnet_Membership]
            WHERE [ApplicationId] = {applicationId} AND [UserId]= {userId}
        ";

        using (var connection = CreateConnection())
        {
            var membership = await connection.QueryFirstAsync<Membership>(query);
            return membership;
        }
    }

    public class User
    {
        public Guid ApplicationId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string LoweredUserName { get; set; }
        public string MobileAlias { get; set; }
        public bool IsAnonymous { get; set; }
        public DateTime LastActivityDate { get; set; }
    }


    public class Membership
    {

        public Guid ApplicationId { get; set; }
        public Guid UserId { get; set; }
        public string Password { get; set; }
        public string PasswordFormat { get; set; }
        public string PasswordSalt { get; set; }
        public string MobilePIN { get; set; }
        public string Email { get; set; }
        public string LoweredEmail { get; set; }
        public string PasswordQuestion { get; set; }

        public string PasswordAnswer { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastLoginDate { get; set; }

        public DateTime LastPasswordChangedDate { get; set; }
        public DateTime LastLockoutDate { get; set; }
        public int FailedPasswordAttemptCount { get; set; }
        public DateTime FailedPasswordAttemptWindowStart { get; set; }

        public int FailedPasswordAnswerAttemptCount { get; set; }

        public DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }

        public string Comment { get; set; }
    }
}

