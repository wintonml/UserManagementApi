namespace UserManagementAPI.Models;

public sealed class CreateUserRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;


    static Dictionary<string, string[]> ValidateUserInput(CreateUserRequest request)
    {
        var errors = new Dictionary<string, List<string>>();

        void Add(string field, string message)
        {
            if (!errors.ContainsKey(field)) errors[field] = new List<string>();
            errors[field].Add(message);
        }

        if (string.IsNullOrWhiteSpace(request.FirstName))
            Add(nameof(request.FirstName), "First name is required.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            Add(nameof(request.LastName), "Last name is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            Add(nameof(request.Email), "Email is required.");
        else if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(request.Email))
            Add(nameof(request.Email), "Email must be a valid address.");

        return errors.ToDictionary(x => x.Key, x => x.Value.ToArray());
    }
}


