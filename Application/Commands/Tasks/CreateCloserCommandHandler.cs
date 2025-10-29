// RealEstate.Application/Commands/Users/CreateCloserCommandHandler.cs
using MediatR;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;
using RealEstate.Domain.Events;
using RealEstate.Domain.Exceptions;
using BCrypt.Net;

namespace RealEstate.Application.Commands.Users;

public class CreateCloserCommandHandler : IRequestHandler<CreateCloserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IEventBus _eventBus;

    public CreateCloserCommandHandler(
        IUserRepository userRepository,
        IEncryptionService encryptionService,
        IEventBus eventBus)
    {
        _userRepository = userRepository;
        _encryptionService = encryptionService;
        _eventBus = eventBus;
    }

    public async Task<UserDto> Handle(CreateCloserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new DuplicateEmailException(request.Email);

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            Agency = request.Agency,
            Status = UserStatus.PendingApproval,
            IsOAuthUser = false
        };

        if (!string.IsNullOrEmpty(request.PhoneNumber))
            user.EncryptedPhoneNumber = _encryptionService.Encrypt(request.PhoneNumber);
        
        user.EncryptedAgency = _encryptionService.Encrypt(request.Agency);

        var userId = await _userRepository.AddAsync(user);
        user.Id = userId;

        await _eventBus.PublishAsync(new CloserApprovalRequestedEvent
        {
            UserId = userId,
            Email = request.Email,
            Name = request.Name
        });

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            PhoneNumber = user.PhoneNumber,
            Agency = user.Agency,
            Status = user.Status,
            IsOAuthUser = user.IsOAuthUser,
            CreatedAt = user.CreatedAt
        };
    }
}