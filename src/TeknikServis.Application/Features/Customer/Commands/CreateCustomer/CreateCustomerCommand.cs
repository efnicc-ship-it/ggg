using MediatR;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Customer.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Phone,
    string? Phone2,
    string? Email,
    string? Address,
    string? City,
    CommunicationPreference CommunicationPreference,
    string? NationalId,   // plaintext — will be encrypted
    bool SmsOptOut,
    bool WhatsAppOptOut,
    bool EmailOptOut
) : IRequest<int>;
