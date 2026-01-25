db = db.getSiblingDB("ForumDB");

["Users", "Categories", "Posts", "Comments"].forEach(c => {
  try { db.createCollection(c); } catch (e) {}
});

db.Posts.createIndex({ TagsIds: 1 });
db.Posts.createIndex({ AuthorId: 1 });
db.Posts.createIndex({ CreatedAt: -1 });

db.Comments.createIndex({ ParentCommentId: 1 });
db.Comments.createIndex({ PostId: 1 });
db.Comments.createIndex({ AuthorId: 1 });

db.Categories.createIndex({ Name: 1 }, { unique: true });

db.Users.createIndex({ username: 1 }, { unique: true });
db.Users.createIndex({ email: 1 }, { unique: true });

print("Init done: collections + indexes created.");
