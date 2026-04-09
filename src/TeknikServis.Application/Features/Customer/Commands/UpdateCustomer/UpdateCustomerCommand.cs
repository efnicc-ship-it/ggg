using MediatR;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Customer.Commands.UpdateCustomer;

public record UpdateCustomerCommand(
    int Id,
    string FirstName,
    string LastName,
    string Phone,
    string? Phone2,
    string? Email,
    string? Address,
    string? City,
    CommunicationPreference CommunicationPreference,
    bool SmsOptOut,
    bool WhatsAppOptOut,
    bool EmailOptOut,
    bool IsBlacklisted,
    string? BlacklistReason
) : IRequest;
