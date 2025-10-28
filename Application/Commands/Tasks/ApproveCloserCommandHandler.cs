// RealEstate.Application/Commands/Users/ApproveCloserCommandHandler.cs
using MediatR;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Enums;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Commands.Users;

public class ApproveCloserCommandHandler : IRequestHandler<ApproveCloserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;

    public ApproveCloserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(ApproveCloserCommand request, CancellationToken cancellationToken)
    {
        var closer = await _userRepository.GetByIdAsync(request.CloserId);
        if (closer == null)
            throw new EntityNotFoundException(nameof(User), request.CloserId);

        if (closer.Status != UserStatus.PendingApproval)
            throw new InvalidOperationException("User is not pending approval.");

        closer.Status = UserStatus.Active;
        closer.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(closer);

        return new UserDto
        {
            Id = closer.Id,
            Email = closer.Email,
            Name = closer.Name,
            PhoneNumber = closer.PhoneNumber,
            Agency = closer.Agency,
            Status = closer.Status,
            IsOAuthUser = closer.IsOAuthUser,
            CreatedAt = closer.CreatedAt
        };
    }
}