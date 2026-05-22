using System;

namespace Fayora.Contracts.AdminModule.GetUsers;

public record GetUsersResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Roles,
    string Status,
    DateTimeOffset CreatedAt
);
