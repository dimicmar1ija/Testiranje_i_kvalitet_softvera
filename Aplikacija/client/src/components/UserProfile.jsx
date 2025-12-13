import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { getUserById } from "../api/userApi";

import PostCard from "../components/PostCard";
import { getPostsByAuthor } from "../api/postApi";

export const UserProfile = () => {
  const { id } = useParams(); 
  const [user, setUser] = useState(null);
  const [posts, setPosts] = useState([]);
  const [loadingPosts, setLoadingPosts] = useState(true);

  useEffect(() => {
    getUserById(id)
      .then(setUser)
      .catch(console.error);
  }, [id]);

  useEffect(() => {
    if (id) {
      setLoadingPosts(true);
      getPostsByAuthor(id)
        .then((data) => setPosts(data))
        .catch(console.error)
        .finally(() => setLoadingPosts(false));
    }
  }, [id]);

  if (!user) return <p className="text-center mt-8">Loading user...</p>;

  return (
    <div className="page-container">
      <div className="profile-card">
        <div className="profile-header">
          <img
            src={user.avatarUrl || "/avatar.png"}
            alt="Avatar"
            className="w-20 h-20 rounded-full object-cover"
          />
          <div>
            <h3 className="text-xl font-bold">{user.username}</h3>
            <p className="text-gray-600">{user.role}</p>
          </div>
        </div>

        <div className="profile-details mt-4">
          <p><strong>Email:</strong> {user.email}</p>
          {user.bio && <p><strong>Bio:</strong> {user.bio}</p>}
          <p><strong>Account created:</strong> {new Date(user.createdAt).toLocaleDateString()}</p>
        </div>
      </div>

      {/* User's posts */}
      <div className="mt-8">
        <h2 className="text-2xl font-bold mb-4 align-center">Postovi napisani od strane: {user.username}</h2>

        {loadingPosts && <p className="text-gray-500">Učitavam postove...</p>}

        {!loadingPosts && posts.length === 0 && (
          <p className="text-gray-500">Ovaj korisnik još uvek nije napisao ni jedan post</p>
        )}

        <div className="flex flex-col gap-4 items-center">
          {posts.map((post) => (
            <div key={post.id || post._id} className="w-1/2">
              <PostCard post={post} />
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};
