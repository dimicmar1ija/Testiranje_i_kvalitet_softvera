import React, { useEffect, useState } from "react";
import { deleteUser, getUsersPreviews } from "../api/userApi";
import { Link } from "react-router";
import { useAuth } from "../context/AuthContext";

export const UsersList = () => {
  const [users, setUsers] = useState([]);
  const { isAdmin } = useAuth(); 

  useEffect(() => {
    getUsersPreviews().then(setUsers).catch(console.error);
  }, []);

  const handleImageError = (e) => {
    e.target.src = "/avatar.png";
  };

  const handleDelete = async (userId) => {
    try {
      await deleteUser(userId);
      setUsers((prevUsers) => prevUsers.filter((u) => u.id !== userId));
    } catch (err) {
      console.error("Failed to delete user:", err);
    }
  };

  return (
    <div className="grid gap-4 p-4 sm:grid-cols-2 lg:grid-cols-3">
      {users.map((user) => (
        <div
          key={user.id}
          className="flex items-center justify-between gap-4 p-4 bg-white rounded-lg shadow-md hover:bg-gray-50 transition"
        >
          <Link
            to={`/users/${user.id}`}
            className="flex items-center gap-4 flex-1"
          >
            <img
              src={user.avatarUrl}
              alt={user.username}
              className="w-12 h-12 rounded-full border"
              onError={handleImageError}
            />
            <div>
              <p className="text-lg font-medium">{user.username}</p>
            </div>
          </Link>

          {isAdmin && (
            <button
              onClick={() => handleDelete(user.id)}
              className="px-3 py-1 text-sm text-white bg-red-500 rounded hover:bg-red-600"
            >
              Obriši
            </button>
          )}
        </div>
      ))}
    </div>
  );
};
