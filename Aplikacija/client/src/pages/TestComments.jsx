import { useEffect, useState } from "react";
import CommentThread from "../components/comments/CommentThread";
import axios from "../api/axiosInstance";
import {
  getAllCategories,
  createCategory,
  deleteCategory,
} from "../api/categoryApi";

export default function TestComments() {

  const [postId, setPostId] = useState("");
  const [activeId, setActiveId] = useState("");

  const [post, setPost] = useState(null);
  const [allCats, setAllCats] = useState([]);
  const [selectedTags, setSelectedTags] = useState([]); // niz categoryId vrednosti

  // UI/help stanja 
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState(null);

  // Admin (kreiranje brisanje kategorija) 
  const [newCat, setNewCat] = useState("");

  const submit = (e) => {
    e.preventDefault();
    setActiveId(postId.trim());
  };
  useEffect(() => {
    const load = async () => {
      if (!activeId) return;
      setLoading(true);
      setErr(null);
      try {
        
        const p = await axios.get(`/post/${activeId}`).then((r) => r.data);
        setPost(p);

        const cats = await getAllCategories();
        setAllCats(cats);

        setSelectedTags(p?.tagsIds || []);
      } catch (e) {
        setErr("Ne mogu da učitam post/kategorije. Proveri da PostId postoji.");
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [activeId]);

  // Toggle za checkbox-e
  const toggleTag = (id) => {
    const set = new Set(selectedTags);
    set.has(id) ? set.delete(id) : set.add(id);
    setSelectedTags(Array.from(set));
  };

  const savePostTags = async () => {
    if (!post) return;
    const updated = {
      ...post,
      tagsIds: selectedTags,
      updatedAt: new Date().toISOString(),
    };
    await axios.put(`/post?id=${post.id}`, updated);
    // osveži post
    const fresh = await axios.get(`/post/${post.id}`).then((r) => r.data);
    setPost(fresh);
  };

  // Admin: kreiraj kategoriju
  const addCategory = async (e) => {
    e.preventDefault();
    if (!newCat.trim()) return;
    setErr(null);
    try {
      await createCategory({ name: newCat.trim() });
      setNewCat("");
      const cats = await getAllCategories();
      setAllCats(cats);
    } catch (ex) {
      if (ex?.response?.status === 409) setErr("Tag sa tim imenom već postoji.");
      else setErr("Greška pri kreiranju taga.");
    }
  };

  // Admin: obriši kategoriju
  const removeCategory = async (id) => {
    await deleteCategory(id);
    const cats = await getAllCategories();
    setAllCats(cats);
    setSelectedTags((prev) => prev.filter((t) => t !== id));
  };

  return (
    <div style={{ textAlign: "left", display: "grid", gap: 20 }}>
      <h2>Test komentara i tagova</h2>

      {/* 1) Izbor posta */}
      <section style={{ display: "grid", gap: 8 }}>
        <h3>1) Izaberi Post</h3>
        <form onSubmit={submit} style={{ display: "flex", gap: 8 }}>
          <input
            placeholder="Nalepi PostId (24-hex ObjectId)"
            value={postId}
            onChange={(e) => setPostId(e.target.value)}
            style={{ flex: 1 }}
          />
          <button type="submit">Učitaj</button>
        </form>
        {err && <div style={{ color: "#e11d48" }}>{err}</div>}
        {activeId && (
          <div style={{ fontSize: 14, color: "#666" }}>
            PostId: <code>{activeId}</code>
          </div>
        )}
      </section>

      {/* 2) Admin panel za kategorije */}
      <section style={{ display: "grid", gap: 8 }}>
        <h3>2) Tagovi – Admin (CRUD)</h3>
        <form onSubmit={addCategory} style={{ display: "flex", gap: 8 }}>
          <input
            placeholder="Novi tag"
            value={newCat}
            onChange={(e) => setNewCat(e.target.value)}
          />
          <button type="submit">Dodaj</button>
        </form>

        <div style={{ display: "grid", gap: 6 }}>
          {allCats.map((c) => (
            <div
              key={c.id}
              style={{
                border: "1px solid #ddd",
                padding: 8,
                borderRadius: 6,
                display: "flex",
                justifyContent: "space-between",
                alignItems: "center",
              }}
            >
              <span>{c.name}</span>
              <button onClick={() => removeCategory(c.id)} style={{ color: "#e11d48" }}>
                Obriši
              </button>
            </div>
          ))}
          {allCats.length === 0 && (
            <div style={{ color: "#666" }}>Nema tagova. Dodaj jedan iznad.</div>
          )}
        </div>
      </section>

      {/* 3) Dodela/čuvanje tagova na izabranom postu */}
      {post && (
        <section style={{ display: "grid", gap: 8 }}>
          <h3>3) Dodeli tagove ovom postu</h3>
          <div style={{ display: "grid", gap: 6 }}>
            {allCats.map((c) => (
              <label key={c.id} style={{ display: "flex", gap: 8, alignItems: "center" }}>
                <input
                  type="checkbox"
                  checked={selectedTags.includes(c.id)}
                  onChange={() => toggleTag(c.id)}
                />
                <span>{c.name}</span>
              </label>
            ))}
          </div>
          <button onClick={savePostTags}>Sačuvaj tagove za post</button>

          <div style={{ fontSize: 13, color: "#555" }}>
            Trenutno dodeljeni tagovi (IDs):{" "}
            {selectedTags.length ? selectedTags.join(", ") : "—"}
          </div>
        </section>
      )}

      {/* 4) Komentari za post */}
      {activeId && (
        <section style={{ display: "grid", gap: 8 }}>
          <h3>4) Komentari</h3>
          {loading ? (
            <div>Učitavanje…</div>
          ) : (
            <CommentThread postId={activeId} />
          )}
        </section>
      )}
    </div>
  );
}
