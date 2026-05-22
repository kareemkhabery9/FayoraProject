using Fayora.Domain.Common.Entity;
using Fayora.Domain.Common.Events.IdentityModule;
using Fayora.Domain.Common.Interfaces.IdentityModule;
using Fayora.Domain.Common.Results;
using Fayora.Domain.Enums.IdentityModule;
using Fayora.Domain.Errors;
using Fayora.Domain.ValueObjects;

namespace Fayora.Domain.Entities.IdentityModule;

public class User : AuditableEntity<Guid>
{
    public static readonly TimeSpan OtpResendCooldown = TimeSpan.FromMinutes(2);
    private static readonly int MaxSmsOtpPerDay = 5;
    private static readonly int MaxEmailOtpPerDay = 10;
    private static readonly TimeSpan AccountLockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly int MaxFailedAccessAttempts = 5;
    private static readonly TimeSpan FailedAccessAttemptWindow = TimeSpan.FromMinutes(15);
    private static readonly int ViolationsBeforeBan = 3;
    private static readonly TimeSpan ViolationWindowForBan = TimeSpan.FromDays(30);

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public DateOnly? BirthDate { get; private set; }
    public Gender? Gender { get; private set; }
    public Email? PrimaryEmail { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public DateTimeOffset? PasswordChangedAt { get; private set; }
    public DateTimeOffset? LastOtpSentAt { get; private set; }
    public DateTimeOffset? LockedUntil { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsPhoneVerified { get; private set; }
    public UserStatus Status { get; private set; } = UserStatus.Active;
    public decimal CurrentBalance { get; private set; } = 0;
    public string? SimCountryIsoCode { get; private set; }
    public Language? PreferredLanguage { get; private set; } = Language.English;
    public string? TimeZone { get; private set; } = string.Empty;
    public FileUrl? ProfileImageUrl { get; private set; }
    public string? Description { get; private set; }
    public string? NationalityCode { get; private set; } = string.Empty;
    public DateTimeOffset? LastLogin { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public DateTimeOffset? LastFailedLoginAt { get; private set; }
    public bool IsProfileComplete { get; private set; }
    public int ViolationCount { get; private set; }
    public int AccessFailedCount { get; private set; }
    public DateTimeOffset? LastViolationDate { get; private set; }

    private readonly List<VerificationCode> _verificationCodes = [];
    public IReadOnlyCollection<VerificationCode> VerificationCodes => _verificationCodes.AsReadOnly();
    public Role? Roles { get; private set; }
    public Language SpokenLanguages =>
    _userLanguageProficiencies.Count != 0
        ? _userLanguageProficiencies
            .Select(x => x.Language)
            .Aggregate((a, b) => a | b)
        : Language.None;


    private string _passwordHash = string.Empty;

    private readonly List<UserLanguageProficiency> _userLanguageProficiencies = [];

    public IReadOnlyCollection<UserLanguageProficiency> UserLanguageProficiencies => _userLanguageProficiencies.AsReadOnly();
    public bool IsVerified => IsEmailVerified || IsPhoneVerified;
    public bool IsLocked => Status == UserStatus.Locked && LockedUntil.HasValue && LockedUntil.Value > DateTimeOffset.UtcNow;
    public bool IsDeleted => Status == UserStatus.Deleted && DeletedAt.HasValue;
    public bool IsBanned => Status == UserStatus.Banned;
    public bool HasPassword => !string.IsNullOrEmpty(_passwordHash);
    public string FullName => $"{FirstName} {LastName}".Trim();

    public static Result<User> CreateWithEmail(
        string firstName,
        string lastName,
        string email,
        string password,
        IPasswordHasher passwordHasher)
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsError) return emailResult.Errors;

        var passwordHashResult = passwordHasher.HashPassword(password);
        if (passwordHashResult.IsError) return passwordHashResult.Errors;

        return new User
        {
            Id = Guid.CreateVersion7(),
            FirstName = firstName,
            LastName = lastName,
            PrimaryEmail = emailResult.Value,
            PhoneNumber = null,
            _passwordHash = passwordHashResult.Value
        };
    }

    public static Result<User> CreateWithPhone(
        string firstName,
        string lastName,
        string phoneNumber,
        string password,
        IPasswordHasher passwordHasher)
    {
        var phoneResult = PhoneNumber.Create(phoneNumber);
        if (phoneResult.IsError) return phoneResult.Errors;

        var passwordHashResult = passwordHasher.HashPassword(password);
        if (passwordHashResult.IsError) return passwordHashResult.Errors;

        return new User
        {
            Id = Guid.CreateVersion7(),
            FirstName = firstName,
            LastName = lastName,
            PrimaryEmail = null,
            PhoneNumber = phoneResult.Value,
            _passwordHash = passwordHashResult.Value
        };
    }

    public static User CreateWithSocialLogin(
    string firstName,
    string lastName,
    string? email,
    string? pictureUrl)
    {
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            IsEmailVerified = true,
            ProfileImageUrl = GetDefaultProfileImageForSocialProvider(pictureUrl),
            FirstName = firstName,
            LastName = lastName
        };

        var emailResult = Email.Create(email);
        if (emailResult.IsSuccess)
            user.PrimaryEmail = emailResult.Value;

        return user;
    }

    public static User CreateBotUser(Guid botId, string firstName, string lastName)
    {
        return new User
        {
            Id = botId,
            FirstName = firstName,
            LastName = lastName,
            Roles = Role.Bot,
            Status = UserStatus.Active,
            IsEmailVerified = true,
            IsPhoneVerified = true,
            CurrentBalance = 0,
            IsProfileComplete = true
        };
    }

    private static FileUrl? GetDefaultProfileImageForSocialProvider(string? pictureUrl)
    {
        var pictureResult = FileUrl.Create(pictureUrl);
        return pictureResult.IsError ? null : pictureResult.Value;
    }


    public void UpdateRegionalPreferences(string? simCountryIso, string? timeZone)
    {
        SimCountryIsoCode = simCountryIso ?? SimCountryIsoCode;
        TimeZone = timeZone ?? TimeZone;
    }

    public void Login() => LastLogin = DateTimeOffset.UtcNow;

    private void RecordPasswordFailure()
    {
        var now = DateTimeOffset.UtcNow;

        if (LastFailedLoginAt.HasValue && now > LastFailedLoginAt.Value.Add(FailedAccessAttemptWindow))
        {
            AccessFailedCount = 1;
        }
        else
        {
            AccessFailedCount++;
        }

        LastFailedLoginAt = now;

        if (AccessFailedCount >= MaxFailedAccessAttempts)
        {
            LockAccount();
        }
    }

    private void ResetAccessStats()
    {
        AccessFailedCount = 0;
        LockedUntil = null;
        LastFailedLoginAt = null;

        if (Status == UserStatus.Locked)
            Status = UserStatus.Active;
    }

    private void LockAccount()
    {
        Status = UserStatus.Locked;
        LockedUntil = DateTimeOffset.UtcNow.Add(AccountLockoutDuration);
    }

    public bool IsCorrectPasswordHash(string password, IPasswordHasher passwordHasher)
    {
        if (passwordHasher.VerifyPassword(password, _passwordHash))
        {
            ResetAccessStats();
            return true;
        }
        else
        {
            RecordPasswordFailure();
            return false;
        }
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        RaiseDomainEvent(new UserSecurityActivityDomainEvent(Id, PrimaryEmail!.Value, SecurityActivityType.EmailVerified));
    }

    public void VerifyPhone() => IsPhoneVerified = true;

    public Result<Success> ChangeEmail(string email)
    {
        var emailResult = Email.Create(email);

        if (emailResult.IsError) return emailResult.Errors;

        var newEmail = emailResult.Value;

        if (PrimaryEmail is not null && PrimaryEmail == newEmail)
            return Result.Success;

        PrimaryEmail = newEmail;
        IsEmailVerified = false;

        Updated();

        RaiseDomainEvent(new UserSecurityActivityDomainEvent(Id, PrimaryEmail.Value, SecurityActivityType.EmailChanged));

        return Result.Success;
    }

    public Result<Success> ChangePhoneNumber(string phoneNumber)
    {
        if (PhoneNumber?.Value == phoneNumber)
            return Result.Success;

        var phoneNumberResult = PhoneNumber.Create(phoneNumber);
        if (phoneNumberResult.IsError) return phoneNumberResult.Errors;

        PhoneNumber = phoneNumberResult.Value;
        IsPhoneVerified = false;

        Updated();
        return Result.Success;
    }

    public void Delete()
    {
        DeletedAt = DateTimeOffset.UtcNow;
        Status = UserStatus.Deleted;
        if (PrimaryEmail is not null)
            RaiseDomainEvent(new UserSecurityActivityDomainEvent(Id, PrimaryEmail.Value, SecurityActivityType.AccountDeleted));
    }

    public void Restore()
    {
        DeletedAt = null;
        Status = UserStatus.Active;
        IsEmailVerified = false;
        IsPhoneVerified = false;
        Updated();
        if (PrimaryEmail is not null)
            RaiseDomainEvent(new UserSecurityActivityDomainEvent(Id, PrimaryEmail.Value, SecurityActivityType.AccountRestored));
    }


    public void UpdateProfile(
        string firstName,
        string lastName,
        DateOnly? birthDate,
        Gender? gender,
        FileUrl? profileImageUrl,
        string? description,
        string? nationalityCode,
        Language? preferredLanguage,
        List<UserLanguageProficiency> userLanguages,
        string? timeZone)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        Gender = gender;

        UpdateProfileImage(profileImageUrl);

        Description = description;
        NationalityCode = nationalityCode;
        PreferredLanguage = preferredLanguage;

        _userLanguageProficiencies.Clear();
        _userLanguageProficiencies.AddRange(userLanguages);

        TimeZone = timeZone;
        IsProfileComplete = CheckIfProfileComplete();

        Updated();
    }

    private void UpdateProfileImage(FileUrl? profileImageUrl)
    {
        if (profileImageUrl != ProfileImageUrl)
        {
            if (ProfileImageUrl is not null)
                RaiseDomainEvent(new DeleteMediaEvent(ProfileImageUrl.ToString()));

            ProfileImageUrl = profileImageUrl;
        }
    }

    private bool CheckIfProfileComplete()
    {
        return !string.IsNullOrWhiteSpace(FirstName)
               && !string.IsNullOrWhiteSpace(LastName)
               && PreferredLanguage.HasValue
               && BirthDate.HasValue
               && Gender.HasValue
               && (PrimaryEmail != null || PhoneNumber != null)
               && ProfileImageUrl is not null;
    }

    public Result<Success> Credit(decimal amount)
    {
        if (amount <= 0)
            return UserErrors.InvalidAmount;

        CurrentBalance += amount;
        return Result.Success;
    }

    public Result<Success> Debit(decimal amount)
    {
        if (amount <= 0)
            return UserErrors.InvalidAmount;

        if (CurrentBalance < amount)
            return UserErrors.InsufficientBalance;

        CurrentBalance -= amount;
        return Result.Success;
    }

    public Result<Success> CanRequestPhoneCode()
    {
        if (CheckOtpCooldown())
            return UserErrors.OtpCooldownNotMet;

        if (HasReachedDailyOtpLimit(MaxSmsOtpPerDay))
            return UserErrors.DailyOtpLimitReached;

        return Result.Success;
    }

    public Result<Success> CanRequestEmailCode()
    {
        if (CheckOtpCooldown())
            return UserErrors.OtpCooldownNotMet;

        if (HasReachedDailyOtpLimit(MaxEmailOtpPerDay))
            return UserErrors.DailyOtpLimitReached;

        return Result.Success;
    }

    private bool CheckOtpCooldown() => LastOtpSentAt.HasValue && DateTimeOffset.UtcNow < LastOtpSentAt.Value.Add(OtpResendCooldown);

    private bool HasReachedDailyOtpLimit(int limit)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-1);
        var last24hCount = _verificationCodes.Count(c => c.CreatedAt > cutoff);
        return last24hCount >= limit;
    }

    public Result<Success> ChangePassword(string newPassword, IPasswordHasher passwordHasher)
    {
        var passwordHashResult = passwordHasher.HashPassword(newPassword);
        if (passwordHashResult.IsError) return passwordHashResult.Errors;

        _passwordHash = passwordHashResult.Value;
        PasswordChangedAt = DateTimeOffset.UtcNow;

        Updated();

        if (PrimaryEmail is not null && IsEmailVerified)
            RaiseDomainEvent(new UserSecurityActivityDomainEvent(Id, PrimaryEmail.Value, SecurityActivityType.PasswordReset));

        return Result.Success;
    }

    public void SendEmailCode(string email, string code, CodePurpose purpose, ICodeHasher codeHasher)
    {
        GenerateAndStoreCode(email, code, purpose, codeHasher);
        RaiseDomainEvent(new EmailCodeRequestedEvent(Id, email, code, purpose));
    }

    public void SendPhoneCode(string phoneNumber, string code, CodePurpose purpose, CodeDeliveryMethod deliveryMethod, ICodeHasher codeHasher)
    {
        GenerateAndStoreCode(phoneNumber, code, purpose, codeHasher);

        RaiseDomainEvent(new PhoneCodeRequestedEvent(Id, phoneNumber, code, purpose, deliveryMethod));
    }

    private void GenerateAndStoreCode(string target, string code, CodePurpose purpose, ICodeHasher codeHasher)
    {
        var codeHash = codeHasher.HashCode(code);
        var verificationCode = VerificationCode.Create(Id, target, codeHash, purpose);
        _verificationCodes.Add(verificationCode);

        LastOtpSentAt = DateTimeOffset.UtcNow;
    }

    public Result<Success> CheckActiveStatus()
    {
        if (IsLocked)
            return Error.Failure("User.UserLocked", $"This user account is locked until {LockedUntil?.ToString("u")}.");

        if (IsDeleted)
            return Error.Failure("User.UserDeleted", "This user account has been deleted.");

        if (IsBanned)
            return Error.Failure("User.UserBanned", "This user account has been banned.");

        return Result.Success;
    }

    public Result<Success> RecordViolation()
    {
        var statusCheck = CheckActiveStatus();
        if (statusCheck.IsError) return statusCheck;

        ViolationCount++;
        LastViolationDate = DateTimeOffset.UtcNow;

        if (ShouldBan())
            Ban();

        Updated();
        return Result.Success;
    }

    private bool ShouldBan()
    {
        if (ViolationCount < ViolationsBeforeBan)
            return false;

        if (LastViolationDate.HasValue &&
            DateTimeOffset.UtcNow - LastViolationDate.Value > ViolationWindowForBan)
        {
            ViolationCount = 1;
            return false;
        }

        return true;
    }

    private void Ban()
    {
        Status = UserStatus.Banned;

        if (PrimaryEmail is not null)
            RaiseDomainEvent(new UserSecurityActivityDomainEvent(
                Id,
                PrimaryEmail.Value,
                SecurityActivityType.AccountBanned));
    }

    public Result<Success> ResetViolations()
    {
        ViolationCount = 0;
        LastViolationDate = null;
        Updated();
        return Result.Success;
    }

    public void AddRole(Role role)
    {
        if (Roles is null)
        {
            Roles = role;
            Updated();
        }
        else if (!Roles?.HasFlag(role) ?? false)
        {
            Roles |= role;
            Updated();
        }
    }

    public void AdminUpdateDetails(string firstName, string lastName, string? email, string? phone)
    {
        FirstName = firstName;
        LastName = lastName;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailResult = Email.Create(email);
            if (emailResult.IsSuccess)
                PrimaryEmail = emailResult.Value;
        }
        if (!string.IsNullOrWhiteSpace(phone))
        {
            var phoneResult = PhoneNumber.Create(phone);
            if (phoneResult.IsSuccess)
                PhoneNumber = phoneResult.Value;
        }
        Updated();
    }

    public void AdminUpdateStatus(UserStatus status)
    {
        Status = status;
        if (status == UserStatus.Active)
        {
            LockedUntil = null;
            AccessFailedCount = 0;
        }
        Updated();
    }

    public void AdminUpdateRoles(Role role)
    {
        Roles = role;
        Updated();
    }

    private User() { }
}