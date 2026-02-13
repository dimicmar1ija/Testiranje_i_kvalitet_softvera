import axios from "./axiosInstance";

export const createPost = (payload) =>
  axios.post("Post", payload).then(r => r.data);

export const getPosts = () =>
  axios.get("Post").then(r => r.data);

export const getPostById = (id) =>
  axios.get(`Post/${id}`).then(r => r.data);

export const deletePost = (id) =>
  axios.delete(`Post/${id}`).then(r => r.data);

export const updatePost = (payload) =>
  axios.put("Post", payload).then(r => r.data);

export const getPostsByAuthor = (authorId) =>
axios.get(`Post/by-author/${authorId}`).then(r => r.data);

export const toggleLikePost = (postId, userId) => {
  return axios.put(`Post/${postId}/like?userId=${userId}`);
};

export const searchPosts = ({ tagsIds = [], match = "any" } = {}) =>
  axios.get("Post/search", {
    params: { tagsIds: tagsIds.join(","), match }
  }).then(r => r.data);
