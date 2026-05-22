namespace Fayora.Contracts.AdminModule.CreateUser;

public record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Password,
    int Roles
);
