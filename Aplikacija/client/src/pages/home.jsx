import { useState } from "react";
import { usePosts } from "../context/PostContext";
import PostCard from "../components/PostCard";
import CreatePostForm from "../components/CreatePostForm";
import TagFilterDropdown from "../components/tags/TagFilterDropdown";
import { searchPosts } from "../api/postApi";

export function Home() {
  const { posts, loading, error, addPost, updatePost } = usePosts();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingPost, setEditingPost] = useState(null);

  const [selectedTags, setSelectedTags] = useState([]); 
  
  const [searching, setSearching] = useState(false);
  const [searchErr, setSearchErr] = useState(null);
  const [results, setResults] = useState([]);

  // Otvori modal za kreiranje novog posta
  const openModalForCreate = () => {
    setEditingPost(null);
    setIsModalOpen(true);
  };

  // Otvori modal za izmenu postojećeg posta
  const openModalForEdit = (post) => {
    setEditingPost(post);
    setIsModalOpen(true);
  };

  const sortedPosts = [...posts].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));

const refreshSearch = async (tags) => {
    setSearchErr(null);
    setSearching(true);
    try {
      const data = await searchPosts({ tagsIds: tags, match: "any" }); // OR logika
      setResults(Array.isArray(data) ? data : []);
      setSelectedTags(tags); 
    } catch (e) {
      setSearchErr("Greška pri pretrazi postova.");
      setResults([]);
    } finally {
      setSearching(false);
    }
  };

 
  const onPostSaved = (post) => {
    if (editingPost) updatePost(post);
    else addPost(post);
    setIsModalOpen(false);
    if (selectedTags.length > 0) refreshSearch(selectedTags);
  };

  const listToShow =
    selectedTags.length > 0
      ? [...results].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
      : sortedPosts;



  return (
    <div div className="p-6" style={{ display: "grid", gridTemplateColumns: "260px 1fr", gap: 24 }}>

      <aside>
        <TagFilterDropdown
          appliedSelected={selectedTags}
          onApply={(sel, mode) => refreshSearch(sel, mode)}
          onReset={() => { setSelectedTags([]); setResults([]); }}
        />
      </aside>

      <main>
        <div className="mt-2 flex flex-col gap-4 w-full items-center">
          {(loading || searching) && <p className="text-gray-500 w-full">Učitavanje postova...</p>}
          {error && <p className="text-red-500 w-full">{error || searchErr}</p>}

          {!loading && !searching && !(error || searchErr) && listToShow.length === 0 && (
              <p className="text-gray-500 w-full">Nema postova za prikaz.</p>
            )}
          {!loading && !searching && !(error || searchErr) && listToShow.length > 0 &&
            listToShow.map((post) => (
              <div key={post.id || post._id} className="w-3/4 mb-4">
                <PostCard post={post} onEdit={openModalForEdit} />
              </div>
            ))
          }
        </div>
      </main>
      {/* Modal za kreiranje/izmenu */}
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
              existingPost={editingPost}
              onPost={(post) => {
                if (editingPost) {
                  updatePost(post); // izmeni postojeći
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
    </div>
  );
}
