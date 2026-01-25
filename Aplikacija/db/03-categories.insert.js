db = db.getSiblingDB("ForumDB");

db.Categories.insertMany([
  { _id: ObjectId("68b0cf24261d6a1b55e4e21d"), Name: "friends" },
  { _id: ObjectId("68ad84342348ed92a368b38e"), Name: "thewitcher" },
  { _id: ObjectId("68ab79b79f299d4be251cafb"), Name: "netflix" },
  { _id: ObjectId("68ac526a6b6bc228cc57b7c7"), Name: "comedy" },
  { _id: ObjectId("68ac3ca1eb221872013e00f1"), Name: "rom-com" },
  { _id: ObjectId("68ab1dab300724193236fc6d"), Name: "godfather" },
  { _id: ObjectId("68ac53c66b6bc228cc57b7ca"), Name: "liveaction" },
  { _id: ObjectId("68ac539c6b6bc228cc57b7c8"), Name: "howtotrainyourdragon" },
  { _id: ObjectId("68b6bc13d167b59b158205be"), Name: "friends-extra" }
]);

print("Categories inserted.");
