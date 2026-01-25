db = db.getSiblingDB("ForumDB");

db.Users.insertMany([
  {
    _id: ObjectId("68b1c138fb185dbf99a07ef7"),
    username: "reactiontime",
    email: "reactiontime@gmail.com",
    passwordHash: "$2a$11$2JRmAWRp6KVpyF5HzdsgU..Ewnz7kgP2Gl8N/AEDsfy4WEhExtfnm",
    role: "user",
    createdAt: ISODate("2025-08-29T15:03:20.766Z"),
    bio: "Nema biografije",
    avatarUrl: "https://stickershop.line-scdn.net/stickershop/v1/product/25982666/LINE…"
  },
  {
    _id: ObjectId("68b6bb8fd167b59b158205bd"),
    username: "user123",
    email: "user@gmail.com",
    passwordHash: "$2a$11$SeXzG89mUHIORJLap95kn.dNGqK6GkTOxHteEVa.aRSmrjuktRfGa",
    role: "user",
    createdAt: ISODate("2025-09-02T09:40:31.594Z"),
    bio: "Nema biografije",
    avatarUrl: "https://example.com/default-avatar.png"
  },
  {
    _id: ObjectId("6945b3fcc5697f0692712211"),
    username: "string00",
    email: "s6h53j50ll@wnbaldwy.com",
    passwordHash: "$2a$11$rN.wNtG4CDYys8FtWlT6ZeP/6jufjEEa7GZWIUG/R5FI3sz5BetUO",
    role: "user",
    createdAt: ISODate("2025-12-19T20:22:20.945Z"),
    bio: "Nema biografije",
    avatarUrl: "https://example.com/default-avatar.png"
  },
  {
    _id: ObjectId("694abbfea567bbc419ed2d00"),
    username: "gg",
    email: "khnzm4yxdw@wnbaldwy.com",
    passwordHash: "$2a$11$5nK6JKdxmB/oopMnKUfxe.A5Xx7bMlk9Bnh1AJGrp94rpSS8egqta",
    role: "user",
    createdAt: ISODate("2025-12-23T15:57:50.172Z"),
    bio: "Nema biografije",
    avatarUrl: "https://example.com/default-avatar.png"
  },
  {
    _id: ObjectId("696f707e533103c71160c595"),
    username: "bree",
    email: "fimamit416@mustaer.com",
    passwordHash: "$2a$11$eWZNl00JxmzUSmHIjUNZluVI83fSmTvzXmreQ/YiBPsod1udsnM9C",
    role: "user",
    createdAt: ISODate("2026-01-20T12:09:34.140Z"),
    bio: "Nema biografije",
    avatarUrl: "https://example.com/default-avatar.png"
  }
]);

print("Users inserted.");
