namespace Store.Domain.Entities;

public class Buyer : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    // EF Core constructor
    private Buyer() { }

    public Buyer(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Buyer name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Buyer email cannot be empty.", nameof(email));

        Name = name;
        Email = email;
    }
}
