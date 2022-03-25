using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

public class MigrationWorkflowConfiguration
{
    [Required]
    public bool IgnoreUserIfEmailIsEmpty { get; set; }

    [Required]
    public bool IgnoreUserIfPasswordIsEmpty { get; set; }

    [Required]
    public bool GeneratePassword { get; set; }

    [Required]
    public string DefaultPassword { get; set; }

    public static MigrationWorkflowConfiguration FromConfiguration(IConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var workflowConfiguration = new MigrationWorkflowConfiguration();
        configuration.GetSection("Workflow").Bind(workflowConfiguration);

        var results = new List<ValidationResult>();

        Validator.TryValidateObject(workflowConfiguration
            , new ValidationContext(workflowConfiguration)
            , results, true);

        if (results.Count > 0)
        {
            foreach (var item in results)
            {
                throw new ArgumentNullException(item.ErrorMessage);
            }
        }

        return workflowConfiguration;
    }
}