import axiosInstance from "./axiosInstance";

// Dobavljanje profila korisnika po ID-u
export const getUserProfile = async (userId) => {
  try {
    const response = await axiosInstance.get(`/User/${userId}`);
    return response.data;
  } catch (error) {
    console.error("Error fetching user profile:", error);
    throw error;
  }
};

// Dobavljanje preview liste korisnika
export const getUsersPreviews = () => {
  return axiosInstance.get(`/User/previews`).then(r => r.data);
}

// Dobavljanje korisnika po ID-u
export const getUserById = (id) => {
  return axiosInstance.get(`/User/${id}`).then(r => r.data);
}

// Brisanje korisnika po ID-u
export const deleteUser = (id) => {
  return axiosInstance.delete(`/User/${id}`).then(r => r.data);
}

export const updateUser = (id, updateUserDto) => {
  return axiosInstance.put(`/User/${id}`, updateUserDto).then(r => r.data);
};
