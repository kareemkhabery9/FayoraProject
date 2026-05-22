namespace Fayora.Domain.Enums.IdentityModule;

[Flags]
public enum Role
{
    Admin = 1,
    Tourist = 2,
    TourGuide = 4,
    TourCompany = 8,
    UnitOwner = 16,
    Support = 32,
    Bot = 64,
}
