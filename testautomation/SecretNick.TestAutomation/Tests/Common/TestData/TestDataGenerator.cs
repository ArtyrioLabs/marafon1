using Bogus;
using Reqnroll;
using Tests.Common.Models;
using Tests.Helpers;

namespace Tests.Common.TestData
{
    public static class TestDataGenerator
    {
        private static readonly Faker _faker = new("uk");

        public static UserCreationDto GenerateUser(bool wantSurprise = false, Table? withData = null)
        {
            var user = new UserCreationDto
            {
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName(),
                Phone = _faker.Phone.PhoneNumber("+380#########"),
                Email = _faker.Internet.Email(),
                DeliveryInfo = _faker.Address.FullAddress(),
                WantSurprise = wantSurprise
            };

            if (wantSurprise)
            {
                user.Interests = _faker.Lorem.Paragraph();
                user.WishList = null;
            }
            else
            {
                user.Interests = null;
                user.WishList = GenerateWishes();
            }

            if (withData != null)
            {
                foreach (var row in withData.Rows)
                {
                    switch (row[0])
                    {
                        case "FirstName":
                            user.FirstName = row[1];
                            break;
                        case "LastName":
                            user.LastName = row[1];
                            break;
                        case "Phone":
                            user.Phone = row[1];
                            break;
                        case "Email":
                            user.Email = row[1];
                            break;
                        case "DeliveryInfo":
                        case "Address":
                            user.DeliveryInfo = row[1];
                            break;
                        case "WantSurprise":
                            user.WantSurprise = bool.Parse(row[1]);
                            break;
                    }
                }
            }

            return user;
        }

        public static RoomCreationDto GenerateRoom(Table? withData = null)
        {
            var room = new RoomCreationDto
            {
                Name = $"Room {_faker.Lorem.Word()} {_faker.Random.Number(1000, 9999)}",
                Description = _faker.Lorem.Sentence(),
                GiftExchangeDate = _faker.Date.Future(1).Simplify(),
                GiftMaximumBudget = _faker.Random.Number(100, 5000)
            };

            if (withData != null)
            {
                foreach (var row in withData.Rows)
                {
                    var field = row["Field"];
                    var value = row["Value"];

                    switch (field)
                    {
                        case "Name":
                            room.Name = value;
                            break;
                        case "Description":
                            room.Description = value;
                            break;
                        case "GiftExchangeDate":
                            if (value == "past")
                                room.GiftExchangeDate = DateTime.UtcNow.AddDays(-1).Simplify();
                            else if (value.StartsWith("future+"))
                            {
                                var days = int.Parse(value.Replace("future+", ""));
                                room.GiftExchangeDate = DateTime.Now.AddDays(days).Simplify();
                            }
                            else
                                room.GiftExchangeDate = DateTime.Parse(value).Simplify();
                            break;
                        case "GiftMaximumBudget":
                            room.GiftMaximumBudget = decimal.Parse(value);
                            break;
                    }
                }
            }

            return room;
        }

        public static List<WishDto> GenerateWishes(int count = 3)
        {
            var wishes = new List<WishDto>();
            for (int i = 0; i < count; i++)
            {
                wishes.Add(GenerateWish());
            }
            return wishes;
        }

        public static WishDto GenerateWish()
        {
            return new WishDto
            {
                Name = _faker.Commerce.ProductName(),
                InfoLink = _faker.Random.Bool() ? $"https://{_faker.Internet.DomainName()}/{_faker.Lorem.Slug()}" : null
            };
        }

        public static string GenerateInvalidFirstName() => new string('A', 41); // 41 chars - exceeds limit of 40
        public static string GenerateInvalidLastName() => new string('B', 41); // 41 chars - exceeds limit of 40
        public static string GenerateInvalidDeliveryAddress() => new string('C', 501); // 501 chars - exceeds limit of 500
    }
}
