using System.Text;
using Microsoft.AspNetCore.Identity;

public class PasswordGenerator
{
    public string? Generate(IdentityOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        int length = options.Password.RequiredLength;

        bool nonAlphanumeric = options.Password.RequireNonAlphanumeric;
        bool digit = options.Password.RequireDigit;
        bool lowercase = options.Password.RequireLowercase;
        bool uppercase = options.Password.RequireUppercase;

        StringBuilder password = new StringBuilder();
        Random random = new Random();

        while (password.Length < length)
        {
            char c = (char)random.Next(32, 126);

            password.Append(c);

            if (char.IsDigit(c))
                digit = false;
            else if (char.IsLower(c))
                lowercase = false;
            else if (char.IsUpper(c))
                uppercase = false;
            else if (!char.IsLetterOrDigit(c))
                nonAlphanumeric = false;
        }

        if (nonAlphanumeric)
            password.Append((char)random.Next(33, 48));

        if (digit)
            password.Append((char)random.Next(48, 58));

        if (lowercase)
            password.Append((char)random.Next(97, 123));

        if (uppercase)
            password.Append((char)random.Next(65, 91));

        return password.ToString();

    }
}
