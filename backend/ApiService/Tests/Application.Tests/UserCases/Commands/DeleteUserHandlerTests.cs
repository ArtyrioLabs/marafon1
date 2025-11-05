using Epam.ItMarathon.ApiService.Application.UseCases.User.Commands;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Handlers;
using Epam.ItMarathon.ApiService.Domain.Abstract;
using Epam.ItMarathon.ApiService.Domain.Entities.User;
using Epam.ItMarathon.ApiService.Domain.Shared.ValidationErrors;
using Epam.ItMarathon.ApiService.Domain.Builders;
using FluentAssertions;
using FluentValidation.Results;
using NSubstitute;

namespace Epam.ItMarathon.ApiService.Application.Tests.UserCases.Commands
{
    /// <summary>
    /// Unit tests for the <see cref="DeleteUserHandler"/> class.
    /// </summary>
    public class DeleteUserHandlerTests
    {
        private readonly IRoomRepository _roomRepositoryMock;
        private readonly IUserReadOnlyRepository _userReadOnlyRepositoryMock;
        private readonly DeleteUserHandler _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteUserHandlerTests"/> class with mocked dependencies.
        /// </summary>
        public DeleteUserHandlerTests()
        {
            _roomRepositoryMock = Substitute.For<IRoomRepository>();
            _userReadOnlyRepositoryMock = Substitute.For<IUserReadOnlyRepository>();
            _handler = new DeleteUserHandler(_roomRepositoryMock, _userReadOnlyRepositoryMock);
        }

        /// <summary>
        /// Tests that the handler returns a NotFoundError when the admin user with userCode is not found.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenAdminUserCodeNotFound()
        {
            // Arrange
            var request = new DeleteUserCommand(UserId: 1, AdminUserCode: "invalid-code");

            _userReadOnlyRepositoryMock
                .GetByCodeAsync(Arg.Any<string>(), CancellationToken.None, true, false)
                .Returns(new NotFoundError([
                    new ValidationFailure("userCode", string.Empty)
                ]));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<NotFoundError>();
            result.Error.Errors.Should().Contain(error =>
                error.PropertyName.Equals("userCode") &&
                error.ErrorMessage.Contains("User with userCode not found"));
        }

        /// <summary>
        /// Tests that the handler returns a ForbiddenError when the user with userCode is not an administrator.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserCodeIsNotAdmin()
        {
            // Arrange
            var nonAdminUser = DataFakers.UserFaker
                .RuleFor(user => user.IsAdmin, _ => false)
                .Generate();
            var request = new DeleteUserCommand(UserId: 2, AdminUserCode: "non-admin-code");

            _userReadOnlyRepositoryMock
                .GetByCodeAsync(request.AdminUserCode, CancellationToken.None, true, false)
                .Returns(nonAdminUser);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ForbiddenError>();
            result.Error.Errors.Should().Contain(error =>
                error.PropertyName.Equals("userCode") &&
                error.ErrorMessage.Contains("not an administrator"));
        }

        /// <summary>
        /// Tests that the handler returns a NotFoundError when the user with id is not found.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserIdNotFound()
        {
            // Arrange
            var adminUser = DataFakers.UserFaker
                .RuleFor(user => user.IsAdmin, _ => true)
                .Generate();
            var request = new DeleteUserCommand(UserId: 999, AdminUserCode: "admin-code");

            _userReadOnlyRepositoryMock
                .GetByCodeAsync(request.AdminUserCode, CancellationToken.None, true, false)
                .Returns(adminUser);

            _userReadOnlyRepositoryMock
                .GetByIdAsync(request.UserId, CancellationToken.None, false, false)
                .Returns(new NotFoundError([
                    new ValidationFailure("id", string.Empty)
                ]));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<NotFoundError>();
            result.Error.Errors.Should().Contain(error =>
                error.PropertyName.Equals("id") &&
                error.ErrorMessage.Contains("User with id not found"));
        }

        /// <summary>
        /// Tests that the handler returns a ForbiddenError when admin and user to delete belong to different rooms.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUsersInDifferentRooms()
        {
            // Arrange
            var adminUser = DataFakers.ValidUserBuilder
                .WithIsAdmin(true)
                .WithRoomId(1)
                .Build();

            var userToDelete = DataFakers.ValidUserBuilder
                .WithRoomId(2)
                .Build();

            var request = new DeleteUserCommand(UserId: userToDelete.Id, AdminUserCode: "admin-code");

            _userReadOnlyRepositoryMock
                .GetByCodeAsync(request.AdminUserCode, CancellationToken.None, true, false)
                .Returns(adminUser);

            _userReadOnlyRepositoryMock
                .GetByIdAsync(request.UserId, CancellationToken.None, false, false)
                .Returns(userToDelete);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ForbiddenError>();
            result.Error.Errors.Should().Contain(error =>
                error.PropertyName.Equals("id") &&
                error.ErrorMessage.Contains("belong to different rooms"));
        }

        /// <summary>
        /// Tests that the handler returns a BadRequestError when admin tries to delete themselves.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenAdminTriesToDeleteThemselves()
        {
            // Arrange
            var adminUser = DataFakers.ValidUserBuilder
                .WithIsAdmin(true)
                .WithRoomId(1)
                .Build();

            var request = new DeleteUserCommand(UserId: adminUser.Id, AdminUserCode: "admin-code");

            _userReadOnlyRepositoryMock
                .GetByCodeAsync(request.AdminUserCode, CancellationToken.None, true, false)
                .Returns(adminUser);

            _userReadOnlyRepositoryMock
                .GetByIdAsync(request.UserId, CancellationToken.None, false, false)
                .Returns(adminUser);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<BadRequestError>();
            result.Error.Errors.Should().Contain(error =>
                error.PropertyName.Equals("id") &&
                error.ErrorMessage.Contains("same user"));
        }

        /// <summary>
        /// Tests that the handler returns a BadRequestError when the room is already closed.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenRoomIsAlreadyClosed()
        {
            // Arrange
            var adminUser = new UserBuilder()
                .WithId(100)
                .WithAuthCode(Guid.NewGuid().ToString())
                .WithFirstName(DataFakers.GeneralFaker.Name.FirstName())
                .WithLastName(DataFakers.GeneralFaker.Name.LastName())
                .WithPhone(DataFakers.GeneralFaker.Phone.PhoneNumber("+380#########"))
                .WithEmail(DataFakers.GeneralFaker.Internet.Email())
                .WithDeliveryInfo(DataFakers.GeneralFaker.Address.StreetAddress())
                .WithWantSurprise(true)
                .WithInterests(DataFakers.GeneralFaker.Lorem.Word())
                .WithWishes([])
                .WithIsAdmin(true)
                .WithRoomId(1)
                .Build();

            var userToDelete = new UserBuilder()
                .WithId(200)
                .WithAuthCode(Guid.NewGuid().ToString())
                .WithFirstName(DataFakers.GeneralFaker.Name.FirstName())
                .WithLastName(DataFakers.GeneralFaker.Name.LastName())
                .WithPhone(DataFakers.GeneralFaker.Phone.PhoneNumber("+380#########"))
                .WithEmail(DataFakers.GeneralFaker.Internet.Email())
                .WithDeliveryInfo(DataFakers.GeneralFaker.Address.StreetAddress())
                .WithWantSurprise(true)
                .WithInterests(DataFakers.GeneralFaker.Lorem.Word())
                .WithWishes([])
                .WithIsAdmin(false)
                .WithRoomId(1)
                .Build();

            var closedRoom = DataFakers.RoomFaker
                .RuleFor(room => room.ClosedOn, faker => faker.Date.Past())
                .RuleFor(room => room.Users, (faker, room) =>
                {
                    var users = new List<User> { adminUser, userToDelete };
                    return users;
                })
                .Generate();

            var request = new DeleteUserCommand(UserId: userToDelete.Id, AdminUserCode: "admin-code");

            _userReadOnlyRepositoryMock
                .GetByCodeAsync(request.AdminUserCode, CancellationToken.None, true, false)
                .Returns(adminUser);

            _userReadOnlyRepositoryMock
                .GetByIdAsync(request.UserId, CancellationToken.None, false, false)
                .Returns(userToDelete);

            _roomRepositoryMock
                .GetByUserCodeAsync(request.AdminUserCode, CancellationToken.None)
                .Returns(closedRoom);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<BadRequestError>();
            result.Error.Errors.Should().Contain(error =>
                error.PropertyName.Equals("room.ClosedOn"));
        }

        /// <summary>
        /// Tests that the handler successfully deletes a user when all validations pass.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldDeleteUserSuccessfully_WhenAllValidationsPass()
        {
            // Arrange
            var adminUser = new UserBuilder()
                .WithId(300)
                .WithAuthCode(Guid.NewGuid().ToString())
                .WithFirstName(DataFakers.GeneralFaker.Name.FirstName())
                .WithLastName(DataFakers.GeneralFaker.Name.LastName())
                .WithPhone(DataFakers.GeneralFaker.Phone.PhoneNumber("+380#########"))
                .WithEmail(DataFakers.GeneralFaker.Internet.Email())
                .WithDeliveryInfo(DataFakers.GeneralFaker.Address.StreetAddress())
                .WithWantSurprise(true)
                .WithInterests(DataFakers.GeneralFaker.Lorem.Word())
                .WithWishes([])
                .WithIsAdmin(true)
                .WithRoomId(1)
                .Build();

            var userToDelete = new UserBuilder()
                .WithId(400)
                .WithAuthCode(Guid.NewGuid().ToString())
                .WithFirstName(DataFakers.GeneralFaker.Name.FirstName())
                .WithLastName(DataFakers.GeneralFaker.Name.LastName())
                .WithPhone(DataFakers.GeneralFaker.Phone.PhoneNumber("+380#########"))
                .WithEmail(DataFakers.GeneralFaker.Internet.Email())
                .WithDeliveryInfo(DataFakers.GeneralFaker.Address.StreetAddress())
                .WithWantSurprise(true)
                .WithInterests(DataFakers.GeneralFaker.Lorem.Word())
                .WithWishes([])
                .WithIsAdmin(false)
                .WithRoomId(1)
                .Build();

            var room = DataFakers.RoomFaker
                .RuleFor(room => room.ClosedOn, _ => null)
                .RuleFor(room => room.Users, (faker, room) =>
                {
                    var users = new List<User> { adminUser, userToDelete };
                    return users;
                })
                .Generate();

            var request = new DeleteUserCommand(UserId: userToDelete.Id, AdminUserCode: "admin-code");

            _userReadOnlyRepositoryMock
                .GetByCodeAsync(request.AdminUserCode, CancellationToken.None, true, false)
                .Returns(adminUser);

            _userReadOnlyRepositoryMock
                .GetByIdAsync(request.UserId, CancellationToken.None, false, false)
                .Returns(userToDelete);

            _roomRepositoryMock
                .GetByUserCodeAsync(request.AdminUserCode, CancellationToken.None)
                .Returns(room);

            _roomRepositoryMock
                .UpdateAsync(Arg.Any<Domain.Aggregate.Room.Room>(), CancellationToken.None)
                .Returns(CSharpFunctionalExtensions.Result.Success());

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
            await _roomRepositoryMock.Received(1).UpdateAsync(Arg.Any<Domain.Aggregate.Room.Room>(), CancellationToken.None);
        }

        /// <summary>
        /// Tests that the handler successfully deletes a user and removes them from the room's user list.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldRemoveUserFromRoomList_WhenDeletionSucceeds()
        {
            // Arrange
            var adminUser = new UserBuilder()
                .WithId(500)
                .WithAuthCode(Guid.NewGuid().ToString())
                .WithFirstName(DataFakers.GeneralFaker.Name.FirstName())
                .WithLastName(DataFakers.GeneralFaker.Name.LastName())
                .WithPhone(DataFakers.GeneralFaker.Phone.PhoneNumber("+380#########"))
                .WithEmail(DataFakers.GeneralFaker.Internet.Email())
                .WithDeliveryInfo(DataFakers.GeneralFaker.Address.StreetAddress())
                .WithWantSurprise(true)
                .WithInterests(DataFakers.GeneralFaker.Lorem.Word())
                .WithWishes([])
                .WithIsAdmin(true)
                .WithRoomId(1)
                .Build();

            var userToDelete = new UserBuilder()
                .WithId(600)
                .WithAuthCode(Guid.NewGuid().ToString())
                .WithFirstName(DataFakers.GeneralFaker.Name.FirstName())
                .WithLastName(DataFakers.GeneralFaker.Name.LastName())
                .WithPhone(DataFakers.GeneralFaker.Phone.PhoneNumber("+380#########"))
                .WithEmail(DataFakers.GeneralFaker.Internet.Email())
                .WithDeliveryInfo(DataFakers.GeneralFaker.Address.StreetAddress())
                .WithWantSurprise(true)
                .WithInterests(DataFakers.GeneralFaker.Lorem.Word())
                .WithWishes([])
                .WithIsAdmin(false)
                .WithRoomId(1)
                .Build();

            var room = DataFakers.RoomFaker
                .RuleFor(room => room.ClosedOn, _ => null)
                .RuleFor(room => room.Users, (faker, room) =>
                {
                    var users = new List<User> { adminUser, userToDelete };
                    return users;
                })
                .Generate();

            var request = new DeleteUserCommand(UserId: userToDelete.Id, AdminUserCode: "admin-code");

            _userReadOnlyRepositoryMock
                .GetByCodeAsync(request.AdminUserCode, CancellationToken.None, true, false)
                .Returns(adminUser);

            _userReadOnlyRepositoryMock
                .GetByIdAsync(request.UserId, CancellationToken.None, false, false)
                .Returns(userToDelete);

            _roomRepositoryMock
                .GetByUserCodeAsync(request.AdminUserCode, CancellationToken.None)
                .Returns(room);

            _roomRepositoryMock
                .UpdateAsync(Arg.Any<Domain.Aggregate.Room.Room>(), CancellationToken.None)
                .Returns(CSharpFunctionalExtensions.Result.Success());

            var initialUserCount = room.Users.Count;

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            room.Users.Should().NotContain(u => u.Id == userToDelete.Id);
            room.Users.Count.Should().Be(initialUserCount - 1);
        }
    }
}
