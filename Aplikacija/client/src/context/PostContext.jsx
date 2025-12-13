import React, { createContext, useContext, useEffect, useState } from "react";
import axios from "axios";

const PostContext = createContext();

export const usePosts = () => useContext(PostContext);

export const PostProvider = ({ children }) => {
  const [posts, setPosts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Funkcija za učitavanje postova sa API-ja
  const fetchPosts = async () => {
    try {
      setLoading(true);
      const res = await axios.get("http://localhost:5132/api/Post");
      setPosts(res.data); // Backend vraća listu postova
      setError(null);
    } catch (err) {
      console.error("Greška pri učitavanju postova:", err);
      setError("Ne mogu da učitam postove");
    } finally {
      setLoading(false);
    }
  };

  // Automatsko učitavanje postova pri mount-u
  useEffect(() => {
    fetchPosts();
  }, []);

  // Dodavanje novog posta lokalno (bez ponovnog fetch-a)
  const addPost = (newPost) => {
    setPosts((prev) => [newPost, ...prev]);
  };

  // Ažuriranje postojećeg posta lokalno
  const updatePost = (updatedPost) => {
    setPosts((prev) =>
      prev.map((post) => (post.id === updatedPost.id ? updatedPost : post))
    );
  };

  return (
    <PostContext.Provider
      value={{ posts, addPost, updatePost, fetchPosts, loading, error }}
    >
      {children}
    </PostContext.Provider>
  );
};
