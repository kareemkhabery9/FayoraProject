namespace Fayora.Contracts.AdminModule.UpdateUser;

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    int Status,
    int Roles
);
