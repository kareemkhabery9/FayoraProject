using Fayora.Domain.Common.Results;
using Fayora.Domain.Entities.IdentityModule;

namespace Fayora.Domain.Errors;

public static class UserErrors
{
    public static readonly Error InvalidEmail = Error.Validation(
        "User.InvalidEmail",
        "The email address is invalid."
    );

    public static readonly Error InvalidPhone = Error.Validation(
        "User.InvalidPhone",
        "The phone number is invalid."
    );

    public static readonly Error EmailOrPhoneRequired = Error.Validation(
        "User.EmailOrPhoneRequired",
        "Either email or phone number must be provided."
    );

    public static readonly Error EmailNotProvided = Error.Validation(
        "User.EmailNotProvided",
        "Email must be provided to verify email."
    );

    public static readonly Error PhoneNotProvided = Error.Validation(
        "User.PhoneNotProvided",
        "Phone number must be provided to verify phone."
    );

    public static readonly Error InvalidPassword = Error.Validation(
        "User.InvalidPassword",
        "The password does not meet the complexity requirements."
    );

    public static readonly Error DeviceIdMissing = Error.Validation(
        "User.DeviceIdMissing",
        "Device ID is missing."
    );

    public static readonly Error EmailBanned = Error.Failure(
        "User.EmailBanned",
        "This email address has been banned."
    );

    public static readonly Error PhoneBanned = Error.Failure(
        "User.PhoneBanned",
        "This phone number has been banned."
    );

    public static readonly Error OtpCooldownNotMet = Error.Failure(
        code: "User.OtpCooldownNotMet",
        description: $"Please wait at least {User.OtpResendCooldown.TotalMinutes} minutes before requesting a new verification code."
    );

    public static readonly Error DailyOtpLimitReached = Error.Failure(
        code: "User.DailyOtpLimitReached",
        description: "You have reached the maximum number of codes allowed per day. Please try again after 24 hours."
    );

    public static readonly Error OnlyOneAllowed = Error.Validation(
        code: "User.OnlyOneAllowed",
        description: "You can provide either Email or Phone Number, not both."
        );

    public static readonly Error InvalidOrExpiredOtp = Error.NotFound(
        code: "VerificationCode.InvalidOrExpired",
        description: "The verification code is invalid, expired, or has not been requested."
    );

    public static readonly Error InvalidResetToken = Error.Validation(
        code: "User.InvalidResetToken",
        description: "The password reset token is invalid or has expired."
    );

    public static readonly Error CodeNotFound = Error.NotFound(
        code: "Authentication.CodeNotFound",
        description: "Verification code not found."
    );

    public static readonly Error InvalidAmount = Error.Validation(
        code: "User.InvalidAmount",
        description: "The amount must be a positive number."
    );

    public static readonly Error InsufficientBalance = Error.Failure(
        code: "User.InsufficientBalance",
        description: "Insufficient balance to complete the transaction."
    );

    public static readonly Error InvalidTarget = Error.Validation(
        code: "User.InvalidTarget",
        description: "The target is invalid."
    );

    public static readonly Error UserDeleted = Error.Failure(
        code: "User.UserDeleted",
        description: "This user has been deleted."
    );

    public static readonly Error InvalidCredentials = Error.Validation(
        code: "User.InvalidCredentials",
        description: "The provided credentials are incorrect."
    );
    public static readonly Error UserNotFound = Error.NotFound(
       code: "User.UserNotFound",
       description: "No user found with the provided information."
   );

    public static readonly Error DuplicateEmail = Error.Conflict(
        code: "User.DuplicateEmail",
        description: "A user with this email already exists."
    );
}