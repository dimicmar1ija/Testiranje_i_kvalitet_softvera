db = db.getSiblingDB("ForumDB");

db.Posts.insertMany([
  // P1: 2 taga, 1 like (reactiontime)
  {
    _id: ObjectId("77f34681e7d9f990ba323701"),
    AuthorId: "68b6bb8fd167b59b158205bd", // user123
    Title: "Koja vam je omiljena epizoda Prijatelja?",
    Body: "Hej svima! Koja vam je omiljena epizoda i zašto? 🙂",
    MediaUrls: ["https://m.media-amazon.com/images/I/81dQwQlmAXL._AC_SL1500_.jpg"],
    TagsIds: ["68b0cf24261d6a1b55e4e21d", "68ac526a6b6bc228cc57b7c7"], // friends + comedy
    LikedByUserIds: ["68b1c138fb185dbf99a07ef7"], // reactiontime
    CreatedAt: ISODate("2025-08-28T21:50:38.832Z"),
    UpdatedAt: ISODate("2025-08-28T21:51:18.625Z")
  },

  // P2: 1 tag, 0 like
  {
    _id: ObjectId("15206a0b1e459a878d9d40cd"),
    AuthorId: "694abbfea567bbc419ed2d00", // gg
    Title: "Witcher",
    Body: "Too bad he is no longer a part of it.",
    MediaUrls: ["https://www.youtube.com/watch?v=kr3br-3i8TY"],
    TagsIds: ["68ad84342348ed92a368b38e"], // thewitcher
    LikedByUserIds: [],
    CreatedAt: ISODate("2025-08-26T09:54:03.741Z"),
    UpdatedAt: ISODate("2025-08-26T09:54:03.741Z")
  },

  // P3: 0 tagova, 3 like-a (pokrije toggle like/unlike)
  {
    _id: ObjectId("8a5a3115db8477382232ee57"),
    AuthorId: "6945b3fcc5697f0692712211", // string00
    Title: "Akcioni i samurajski filmovi – preporuke?",
    Body: "Preporučite mi dobre samurajske i akcione filmove, hvala! ⚔️",
    MediaUrls: [],
    TagsIds: [],
    LikedByUserIds: [
      "68b1c138fb185dbf99a07ef7", // reactiontime
      "68b6bb8fd167b59b158205bd", // user123
      "696f707e533103c71160c595"  // bree
    ],
    CreatedAt: ISODate("2025-08-27T17:52:20.004Z"),
    UpdatedAt: ISODate("2025-08-27T17:52:20.004Z")
  },

  // P4: 2 taga (netflix + rom-com), edited (UpdatedAt kasnije)
  {
    _id: ObjectId("72ec184274d77d5cd7e1b481"),
    AuthorId: "696f707e533103c71160c595", // bree
    Title: "The Godfather (diskusija)",
    Body: "Widely regarded as one of the greatest films of all time. Šta vi mislite?",
    MediaUrls: ["http://google.com/search?tbm=isch&q=Godfather"],
    TagsIds: ["68ab1dab300724193236fc6d", "68ab79b79f299d4be251cafb"], // godfather + netflix
    LikedByUserIds: [],
    CreatedAt: ISODate("2025-08-25T15:58:38.430Z"),
    UpdatedAt: ISODate("2025-08-25T16:10:00.000Z")
  },

  // P5: 1 tag friends-extra, bez media
  {
    _id: ObjectId("64ef05c308bd8ad5e6c297af"),
    AuthorId: "68b1c138fb185dbf99a07ef7", // reactiontime
    Title: "Friends",
    Body: "Da li je neko gledao ponovo celu seriju? 🙂",
    MediaUrls: [],
    TagsIds: ["68b6bc13d167b59b158205be"], // friends-extra
    LikedByUserIds: [],
    CreatedAt: ISODate("2025-09-02T09:42:47.642Z"),
    UpdatedAt: ISODate("2025-09-02T09:42:47.642Z")
  }
]);

print("Posts inserted.");
