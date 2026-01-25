db = db.getSiblingDB("ForumDB");

db.Comments.insertMany([
  // C1: root comment na P1, like od bree
  {
    _id: ObjectId("68ad897bfc39e7b3577f6419"),
    PostId: "77f34681e7d9f990ba323701",
    AuthorId: "6945b3fcc5697f0692712211", // string00
    ParentCommentId: null,
    Body: "Meni je omiljena epizoda ona sa pivom u Londonu 😂",
    CreatedAt: ISODate("2025-08-29T10:00:00.000Z"),
    UpdatedAt: ISODate("2025-08-29T10:00:00.000Z"),
    LikedByUserIds: ["696f707e533103c71160c595"], // bree
    DislikedByUserIds: []
  },

  // C2: reply na C1, dislike od user123 (da testiraš undislike)
  {
    _id: ObjectId("68af46a3b25ee25a5a1e1ee5"),
    PostId: "77f34681e7d9f990ba323701",
    AuthorId: "68b1c138fb185dbf99a07ef7", // reactiontime
    ParentCommentId: "68ad897bfc39e7b3577f6419",
    Body: "Hahaha dobra! Meni je sa 'we were on a break' 😄",
    CreatedAt: ISODate("2025-08-29T10:05:00.000Z"),
    UpdatedAt: ISODate("2025-08-29T10:05:00.000Z"),
    LikedByUserIds: [],
    DislikedByUserIds: ["68b6bb8fd167b59b158205bd"] // user123
  },

  // C3: root comment na P2, bez reakcija
  {
    _id: ObjectId("68af4620b25ee25a5a1e1ee3"),
    PostId: "15206a0b1e459a878d9d40cd",
    AuthorId: "68b6bb8fd167b59b158205bd", // user123
    ParentCommentId: null,
    Body: "Meni je prva sezona bila najbolja.",
    CreatedAt: ISODate("2025-08-26T12:00:00.000Z"),
    UpdatedAt: ISODate("2025-08-26T12:00:00.000Z"),
    LikedByUserIds: [],
    DislikedByUserIds: []
  },

  // C4: root comment na P3, like od gg i dislike od bree (da testiraš prelazak like->dislike)
  {
    _id: ObjectId("68af466ab25ee25a5a1e1ee4"),
    PostId: "8a5a3115db8477382232ee57",
    AuthorId: "694abbfea567bbc419ed2d00", // gg
    ParentCommentId: null,
    Body: "Pogledaj '13 Assassins' i 'Harakiri' — vrh!",
    CreatedAt: ISODate("2025-08-27T18:00:00.000Z"),
    UpdatedAt: ISODate("2025-08-27T18:00:00.000Z"),
    LikedByUserIds: ["694abbfea567bbc419ed2d00"], // gg (sam sebi - dozvoljeno za test, nije bitno)
    DislikedByUserIds: ["696f707e533103c71160c595"] // bree
  }
]);

print("Comments inserted (seed, threaded + likes/dislikes).");
