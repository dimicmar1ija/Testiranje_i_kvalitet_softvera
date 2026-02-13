import axios from "./axiosInstance";

// GET threaded komentari za post
export const getThreadedByPost = (postId) =>
  axios.get(`comment/post/${postId}/threaded`).then(r => r.data);

// POST kreiraj komentar (dto: { postId, authorId, body, parentCommentId? })
export const createComment = (payload) =>
  axios.post(`comment`, payload).then(r => r.data);

// PUT izmeni komentar (dto: { body })
export const updateComment = (id, payload) =>
  axios.put(`comment/${id}`, payload);

// DELETE komentar
export const deleteComment = (id, token) =>
  axios.delete(`comment/${id}` , {
     headers: { Authorization: `Bearer ${token}` }
  });

// like / dislike / undo
export const likeComment = (id, userId) =>
  axios.post(`comment/${id}/like/${userId}`);

export const dislikeComment = (id, userId) =>
  axios.post(`comment/${id}/dislike/${userId}`);

export const unlikeComment = (id, userId) =>
  axios.post(`comment/${id}/unlike/${userId}`);

export const undislikeComment = (id, userId) =>
  axios.post(`comment/${id}/undislike/${userId}`);

