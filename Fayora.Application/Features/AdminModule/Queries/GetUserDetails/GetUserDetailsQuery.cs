using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;
using System;

namespace Fayora.Application.Features.AdminModule.Queries.GetUserDetails;

public record GetUserDetailsQuery(Guid UserId) : IQuery<Result<UserDetailsResult>>;

public record UserDetailsResult(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Roles,
    string Status,
    decimal CurrentBalance,
    string PreferredLanguage,
    string ProfileImageUrl,
    DateTimeOffset CreatedAt,
    TouristProfileDto? TouristProfile,
    TourGuideProfileDto? TourGuideProfile,
    TourCompanyProfileDto? TourCompanyProfile
);

public record TouristProfileDto(
    string Nationality,
    string PreferredCurrency,
    int BookingsCount
);

public record TourGuideProfileDto(
    string City,
    decimal Rating,
    int ToursHosted,
    string VerificationStatus
);

public record TourCompanyProfileDto(
    string CompanyName,
    string CommercialRegistrationNumber,
    decimal Rating,
    int PackagesCreated,
    string VerificationStatus
);
