import { useContext, useState } from "react";
import { useNavigate } from "react-router-dom";
import { AuthContext } from "../context/AuthContext";
import { usePosts } from "../context/PostContext";
import CreatePostForm from "../components/CreatePostForm";

const Header = () => {
  const navigate = useNavigate();
  const { logout } = useContext(AuthContext);
  const { addPost, updatePost } = usePosts();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingPost, setEditingPost] = useState(null); // post koji se edit-uje

  const openModalForEdit = (post) => {
    setEditingPost(post);
    setIsModalOpen(true);
  };

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <>
      <header className="fixed top-0 left-0 w-full flex justify-between items-center bg-yellow-600 text-white px-4 py-3 z-50 shadow-md">
        <h1
          className="text-xl font-bold flex items-center cursor-pointer"
          onClick={() => navigate("/home")}
        >
          Filmski Kutak <span role="img" aria-label="film-reel" className="ml-2">🎬</span>
        </h1>

        <div className="flex space-x-2">
          <button
            onClick={() => { setEditingPost(null); setIsModalOpen(true); }}
            className="bg-blue-500 hover:bg-blue-600 px-3 py-1 rounded"
          >
            Kreiraj post
          </button>
          <button
            onClick={handleLogout}
            className="bg-red-500 hover:bg-red-600 px-3 py-1 rounded"
          >
            Odjavi se
          </button>
          <button
            onClick={() => navigate("/profile")}
            className="bg-gray-500 hover:bg-gray-600 px-3 py-1 rounded"
          >
            Moj profil
          </button>
        </div>
      </header>

      {/* Modal za kreiranje/izmenu posta */}
      {isModalOpen && (
        <div className="fixed inset-0 backdrop-blur-sm bg-black bg-opacity-50 flex justify-center items-center z-50">
          <div className="bg-white rounded-lg shadow-lg p-6 w-96 relative">
            <button
              onClick={() => setIsModalOpen(false)}
              className="absolute top-2 right-2 text-gray-600 hover:text-black"
            >
              ✖
            </button>
            <h2 className="text-xl font-bold mb-4">
              {editingPost ? "Izmeni post" : "Kreiraj novi post"}
            </h2>
            <CreatePostForm
              existingPost={editingPost} // prosleđuje se post za edit
              onPost={(post) => {
                if (editingPost) {
                  updatePost(post); // izmeni u context-u
                } else {
                  addPost(post);    // dodaj novi
                }
                setIsModalOpen(false);
              }}
              onCancel={() => setIsModalOpen(false)}
            />
          </div>
        </div>
      )}
    </>
  );
};

export default Header;
