using AutoMapper;
using Fayora.Application.Features.AccommodationModule.Commands.CreateUnit;
using Fayora.Application.Features.AccommodationModule.Commands.UpdateUnit;
using Fayora.Application.Features.AccommodationModule.Commands.DeleteUnit;
using Fayora.Application.Features.AccommodationModule.Commands.CreateUnitCalendarBlock;
using Fayora.Application.Features.AccommodationModule.Commands.CreateUnitOwner;
using Fayora.Application.Features.AccommodationModule.Queries.GetAllAmenities;
using Fayora.Application.Features.AccommodationModule.Queries.GetRecommendedUnits;
using Fayora.Application.Features.AccommodationModule.Queries.GetUnitById;
using Fayora.Application.Features.AccommodationModule.Queries.GetUnits;
using Fayora.Contracts.AccommodationModule.Requests;
using Fayora.Contracts.AccommodationModule.Responses;
using Fayora.Domain.Enums.AccommodationModule;
using Fayora.Domain.Enums.BookingModule;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fayora.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccommodationController(ISender sender, IMapper mapper) : ApiController
{
    [HttpGet("amenities")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllAmenitiesQuery();
        var result = await sender.Send(query, cancellationToken);

        return result.Match(
            amenities => Ok(mapper.Map<List<Amenity>>(amenities)),
            error => Problem(error)
        );
    }

    [HttpPost("owner-profile")]
    public async Task<IActionResult> CreateUnitOwnerProfileAsync(
    [FromHeader(Name = AppHeaders.DeviceId)] string deviceId,
    [FromBody] CreateUnitOwnerProfileRequest request,
    CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<UnitOwnerType>(request.OwnerType, true, out var ownerType))
        {
            return BadRequest("Invalid Unit Owner Type.");
        }

        var command = new CreateUnitOwnerCommand(
            deviceId,
            ownerType,
            request.CommercialName);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            value => Ok(mapper.Map<CreateUnitOwnerProfileResponse>(value)),
            errors => Problem()
        );
    }

    [HttpPost("housing-units")]
    public async Task<IActionResult> CreateUnit(
    [FromBody] CreateUnitRequest request,
    CancellationToken cancellationToken)
    {
        var command = new CreateUnitCommand(
            request.Title,
            request.Description,
            request.LocationId,
            request.AddressDetails,
            request.Latitude,
            request.Longitude,
            Enum.Parse<HousingType>(request.Type, true),
            request.PricePerNight,
            request.NumberOfRooms,
            request.BedRooms,
            request.BathRooms,
            request.NumberOfBeds,
            request.MaxGuests,
            request.CheckInTime,
            request.CheckOutTime,
            request.MainImageUrl,
            request.VerificationDocumentUrl,
            request.ImageUrls,
            [.. request.AmenityIds]
        );

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            value => Ok(value),
            errors => Problem(errors)
        );
    }

    [HttpPut("housing-units/{id:guid}")]
    public async Task<IActionResult> UpdateUnit(
        [FromRoute] Guid id,
        [FromBody] UpdateUnitRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<HousingType>(request.Type, true, out var housingType))
        {
            return BadRequest("Invalid housing type.");
        }

        var command = new UpdateUnitCommand(
            id,
            request.Title,
            request.Description,
            request.LocationId,
            request.AddressDetails,
            request.Latitude,
            request.Longitude,
            housingType,
            request.PricePerNight,
            request.NumberOfRooms,
            request.BedRooms,
            request.BathRooms,
            request.NumberOfBeds,
            request.MaxGuests,
            request.CheckInTime,
            request.CheckOutTime,
            request.MainImageUrl,
            request.ImageUrls,
            [.. request.AmenityIds]
        );

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            value => Ok(value),
            errors => Problem(errors)
        );
    }

    [HttpDelete("housing-units/{id:guid}")]
    public async Task<IActionResult> DeleteUnit(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteUnitCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors)
        );
    }

    [HttpPost("{unitId:guid}/blocks")]
    public async Task<IActionResult> CreateCalendarBlock(
        [FromRoute] Guid unitId,
        [FromBody] CreateCalendarBlockRequest request,
        CancellationToken cancellationToken)
    {
        var (blockOk, blockReason) = EnumParser.TryParseEnum<BlockReason>(request.BlockReason);

        if (!blockOk)
        {
            return BadRequest("Invalid Block Reason.");
        }

        var command = new CreateUnitCalendarBlockCommand(
            unitId,
            request.StartDate,
            request.EndDate,
            blockReason
        );

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            _ => (IActionResult)NoContent(),
            errors => Problem()
        );
    }


    [HttpGet("housing-units/{id:guid}")]
    public async Task<IActionResult> GetUnitById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetUnitByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return result.Match(
            value => Ok(mapper.Map<GetUnitByIdResponse>(value)),
            errors => Problem()
        );
    }

    //[HttpPost("housing-units/{id:guid}/views")]
    //public async Task<IActionResult> IncrementUnitViews(
    //    [FromRoute] Guid id,
    //    CancellationToken cancellationToken)
    //{
    //    var command = new IncrementUnitViewsCommand(id);

    //    var result = await sender.Send(command, cancellationToken);

    //    return result.Match(
    //        _ => NoContent(), // 
    //        errors => Problem()
    //    );
    //}



    [HttpGet("housing-units")]
    public async Task<IActionResult> GetUnitsAsync(
    [FromQuery] string? type,
    [FromQuery] string? searchTerm,
    CancellationToken cancellationToken)
    {
        HousingType? housingType = null;
        if (!string.IsNullOrWhiteSpace(type))
        {
            if (!Enum.TryParse<HousingType>(type, true, out var parsed))
                return BadRequest("Invalid housing type. Valid values are: Apartment, Villa, Hotel.");
            housingType = parsed;
        }

        var query = new GetUnitsQuery(housingType, searchTerm);

        var result = await sender.Send(query, cancellationToken);

        return result.Match(
            value => Ok(value),
            errors => Problem()
        );
    }

    /// <summary>
    /// Returns personalized housing unit recommendations for authenticated users,
    /// or trending units for anonymous users.
    /// Uses a multi-signal scoring engine (budget/travel-style matching, collaborative, popularity, recency).
    /// </summary>
    [HttpGet("recommended")]
    public async Task<IActionResult> GetRecommendedUnits(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRecommendedUnitsQuery(count);
        var result = await sender.Send(query, cancellationToken);
        return result.Match(
            value => Ok(value),
            errors => Problem()
        );
    }
}