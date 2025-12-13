import axios from "./axiosInstance";

export const getAllCategories = () =>
  axios.get(`/category`).then(r => r.data);

export const getCategory = (id) =>
  axios.get(`/category/${id}`).then(r => r.data);

export const createCategory = (payload) => // { name }
  axios.post(`/category`, payload).then(r => r.data);

export const deleteCategory = (id) =>
  axios.delete(`/category/${id}`);
