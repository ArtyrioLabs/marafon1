using CSharpFunctionalExtensions;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Commands;
using Epam.ItMarathon.ApiService.Domain.Abstract;
using Epam.ItMarathon.ApiService.Domain.Shared.ValidationErrors;
using FluentValidation.Results;
using MediatR;

namespace Epam.ItMarathon.ApiService.Application.UseCases.User.Handlers
{
    /// <summary>
    /// Handler for User deletion from the Room.
    /// </summary>
    /// <param name="roomRepository">Implementation of <see cref="IRoomRepository"/> for operating with database.</param>
    /// <param name="userRepository">Implementation of <see cref="IUserReadOnlyRepository"/> for operating with database.</param>
    public class DeleteUserHandler(IRoomRepository roomRepository, IUserReadOnlyRepository userRepository) :
        IRequestHandler<DeleteUserCommand, Result<bool, ValidationResult>>
    {
        ///<inheritdoc/>
        public async Task<Result<bool, ValidationResult>> Handle(DeleteUserCommand request,
            CancellationToken cancellationToken)
        {
            // Validate admin user exists
            var adminUserResult = await userRepository.GetByCodeAsync(request.AdminUserCode, cancellationToken,
                includeRoom: true);
            if (adminUserResult.IsFailure)
            {
                return Result.Failure<bool, ValidationResult>(new NotFoundError([
                    new ValidationFailure("userCode", "User with userCode not found.")
                ]));
            }

            var adminUser = adminUserResult.Value;

            // Validate admin user is admin
            if (!adminUser.IsAdmin)
            {
                return Result.Failure<bool, ValidationResult>(new ForbiddenError([
                    new ValidationFailure("userCode", "User with userCode is not an administrator.")
                ]));
            }

            // Validate user to delete exists
            var userToDeleteResult = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (userToDeleteResult.IsFailure)
            {
                return Result.Failure<bool, ValidationResult>(new NotFoundError([
                    new ValidationFailure("id", "User with id not found.")
                ]));
            }

            var userToDelete = userToDeleteResult.Value;

            // Validate users belong to the same room
            if (adminUser.RoomId != userToDelete.RoomId)
            {
                return Result.Failure<bool, ValidationResult>(new ForbiddenError([
                    new ValidationFailure("id", "User with userCode and user with id belong to different rooms.")
                ]));
            }

            // Validate admin is not trying to delete themselves
            if (adminUser.Id == userToDelete.Id)
            {
                return Result.Failure<bool, ValidationResult>(new BadRequestError([
                    new ValidationFailure("id", "User with userCode and id is the same user.")
                ]));
            }

            // Get room and validate it's not closed
            var roomResult = await roomRepository.GetByUserCodeAsync(request.AdminUserCode, cancellationToken);
            if (roomResult.IsFailure)
            {
                return roomResult.ConvertFailure<bool>();
            }

            var room = roomResult.Value;

            // Room closed check is done in RemoveUser method
            var removeUserResult = room.RemoveUser(request.UserId);
            if (removeUserResult.IsFailure)
            {
                return removeUserResult.ConvertFailure<bool>();
            }

            // Update room in database
            var updateResult = await roomRepository.UpdateAsync(removeUserResult.Value, cancellationToken);
            if (updateResult.IsFailure)
            {
                return Result.Failure<bool, ValidationResult>(new BadRequestError([
                    new ValidationFailure("room", updateResult.Error)
                ]));
            }

            return true;
        }
    }
}
