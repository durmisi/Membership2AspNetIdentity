public partial class Membership2AspNetIdentityApiClient
{
    public class MembershipPasswordResult
    {
        public string Password { get; set; }

        public bool IsLockedOut { get; set; }

        public bool Pex { get; set; }

        public string PexMessage { get; set; }
    }
}

