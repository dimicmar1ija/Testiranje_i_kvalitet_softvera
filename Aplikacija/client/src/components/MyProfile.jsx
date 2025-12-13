import React, { useState, useEffect } from "react";
import { useAuth } from "../context/AuthContext";
import { UsersList } from "./UserPreview";
import { updateUser, deleteUser } from "../api/userApi";
import { useNavigate } from "react-router-dom";

export const MyProfile = () => {
  const { user, loading, logout } = useAuth(); // Assuming logout clears auth
  const navigate = useNavigate();
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState({
    email: "",
    username: "",
    bio: "",
    avatarUrl: ""
  });
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    if (user) {
      setFormData({
        email: user.email ?? "",
        username: user.username ?? "",
        bio: user.bio ?? "",
        avatarUrl: user.avatarUrl ?? ""
      });
    }
  }, [user]);

  if (loading)
    return <p className="text-center text-gray-400 mt-8">Loading profile...</p>;
  if (!user)
    return <p className="text-center text-gray-400 mt-8">No profile data available.</p>;

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      await updateUser(user.id, formData);
      setIsEditing(false);
    } catch (err) {
      console.error("Error updating profile:", err);
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    setFormData({
      email: user.email ?? "",
      username: user.username ?? "",
      bio: user.bio ?? "",
      avatarUrl: user.avatarUrl ?? ""
    });
    setIsEditing(false);
  };

  const handleDelete = async () => {
    if (!confirm("Da li ste sigurni da želite da obrišete svoj profil?")) return;
    try {
      setDeleting(true);
      await deleteUser(user.id);
      logout(); // Clear authentication
      navigate("/"); // Redirect to home or login page
    } catch (err) {
      console.error("Error deleting profile:", err);
    } finally {
      setDeleting(false);
    }
  };

  return (
    <div className="page-container">
      {/* Profile Section */}
      <div className="profile-card">
        <h2>Moj profil</h2>

        <div className="profile-header">
          <img
            src={user.avatarUrl || "/avatar.png"}
            alt="Avatar"
            className="w-20 h-20 rounded-full object-cover"
          />
          <div>
            <h3>{user.username}</h3>
            <p>{user.role}</p>
          </div>
        </div>

        <div className="profile-details mt-4">
          <p><strong>Email:</strong> {user.email}</p>
          {user.bio && <p><strong>Bio:</strong> {user.bio}</p>}
          <p><strong>Profil kreiran:</strong> {new Date(user.createdAt).toLocaleDateString()}</p>
        </div>

        <div className="mt-4">
          {isEditing ? (
            <div className="edit-form space-y-3">
              <label className="block">
                <span className="font-semibold">Email:</span>
                <input
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  className="w-full border rounded px-2 py-1 mt-1"
                />
              </label>
              <label className="block">
                <span className="font-semibold">Korisničko ime:</span>
                <input
                  type="text"
                  name="username"
                  value={formData.username}
                  onChange={handleChange}
                  className="w-full border rounded px-2 py-1 mt-1"
                />
              </label>
              <label className="block">
                <span className="font-semibold">Bio:</span>
                <textarea
                  name="bio"
                  value={formData.bio}
                  onChange={handleChange}
                  className="w-full border rounded px-2 py-1 mt-1"
                />
              </label>
              <label className="block">
                <span className="font-semibold">Avatar URL:</span>
                <input
                  type="text"
                  name="avatarUrl"
                  value={formData.avatarUrl}
                  onChange={handleChange}
                  className="w-full border rounded px-2 py-1 mt-1"
                />
              </label>

              <div className="flex gap-2 mt-3">
                <button
                  onClick={handleSave}
                  disabled={saving}
                  className="bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600"
                >
                  {saving ? "Saving..." : "Save"}
                </button>
                <button
                  onClick={handleCancel}
                  className="bg-gray-400 text-white px-4 py-2 rounded hover:bg-gray-500"
                >
                  Cancel
                </button>
              </div>
            </div>
          ) : (
            <div className="flex gap-2">
              <button
                onClick={() => setIsEditing(true)}
                className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
              >
                Izmeni profil
              </button>
              <button
                onClick={handleDelete}
                disabled={deleting}
                className="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-600"
              >
                {deleting ? "Brišem..." : "Obriši profil"}
              </button>
            </div>
          )}
        </div>
      </div>

      {/* Other Users Section */}
      <div className="other-users mt-8">
        <h2 className="text-2xl font-bold mb-4">Drugi korisnici</h2>
        <UsersList />
      </div>
    </div>
  );
};
