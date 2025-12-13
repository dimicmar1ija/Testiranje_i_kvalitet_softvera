import React from "react";
import CreatePostForm from "../components/CreatePostForm";
import PostCard from "../components/PostCard";
import { usePosts } from "../context/PostContext";

export function CreatePost() {
  const { posts, addPost } = usePosts();

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold mb-4">Kreiraj novi post</h1>

      <CreatePostForm onPost={addPost} />

      <div className="mt-6 space-y-4">
        {posts.length === 0 ? (
          <p className="text-gray-500">Nema postova za prikaz.</p>
        ) : (
          posts.map((post, i) => <PostCard key={i} post={post} />)
        )}
      </div>
    </div>
  );
}
