import React, { useState, useEffect, useContext } from "react";
import { getAllCategories, createCategory } from "../api/categoryApi"; 
import { createPost as apiCreatePost } from "../api/postApi";           
import { AuthContext } from "../context/AuthContext";                   
import { jwtDecode } from "jwt-decode";
import { updatePost } from "../api/postApi";
import http from "../api/axiosInstance";


export default function CreatePostForm({ onPost, onCancel, existingPost }) {
  const { token } = useContext(AuthContext);

  // --- State ---
  const [title, setTitle] = useState(existingPost?.title ?? "");
  const [body, setBody] = useState(existingPost?.body ?? "");
  const [mediaUrls, setMediaUrls] = useState(existingPost?.mediaUrls?.join(", ") ?? "");
  const [categories, setCategories] = useState([]);
  const [selectedTagIds, setSelectedTagIds] = useState(existingPost?.tagsIds ?? []);
  const [tagInput, setTagInput] = useState("");
  const [loadingCats, setLoadingCats] = useState(false);
  const [posting, setPosting] = useState(false);
  const [err, setErr] = useState(null);

  // --- Load categories ---
  useEffect(() => {
    const loadCategories = async () => {
      setLoadingCats(true);
      try {
        const data = await getAllCategories();
        setCategories(data);
      } finally {
        setLoadingCats(false);
      }
    };
    loadCategories();
  }, []);

  // --- Toggle tag selection ---
  const toggleTag = (id) => {
    setSelectedTagIds((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]
    );
  };

  const byName = (arr, name) => arr.find((c) => c.name.toLowerCase() === name.toLowerCase());

  // --- Ensure tag exists or create it ---
  const ensureTagCreated = async (name) => {
    const trimmed = name.trim();
    if (!trimmed) return null;

    const existing = byName(categories, trimmed);
    if (existing) {
      setSelectedTagIds((prev) =>
        prev.includes(existing.id) ? prev : [...prev, existing.id]
      );
      return existing.id;
    }

    try {
      const created = await createCategory({ name: trimmed });
      setCategories((prev) => [...prev, created]);
      setSelectedTagIds((prev) => [...prev, created.id]);
      return created.id;
    } catch (err) {
      // Handle race condition (409 conflict)
      if (err?.response?.status === 409) {
        const latest = await getAllCategories();
        setCategories(latest || []);
        const cat = byName(latest || [], trimmed);
        if (cat) {
          setSelectedTagIds((prev) =>
            prev.includes(cat.id) ? prev : [...prev, cat.id]
          );
          return cat.id;
        }
      }
      throw err;
    }
  };

  const handleAddTag = async (e) => {
    if (e.key !== "Enter") return;
    e.preventDefault();
    if (!tagInput.trim()) return;

    try {
      await ensureTagCreated(tagInput);
      setTagInput("");
    } catch {
      alert("Greška pri dodavanju taga.");
    }
  };

  const generateObjectId = () => {
    let hex = "";
    for (let i = 0; i < 24; i++) hex += Math.floor(Math.random() * 16).toString(16);
    return hex;
  };

  // --- Handle submit (create or update) ---
  const handleSubmit = async (e) => {
    e.preventDefault();
    setErr(null);

    if (!token) {
      setErr("Niste ulogovani.");
      return;
    }

    let sub;
    try {
      sub = jwtDecode(token)?.sub;
    } catch {}

    if (!sub || !String(sub).trim()) {
      setErr("Ne mogu da očitam authorId iz tokena.");
      return;
    }

    if (!title.trim()) {
      setErr("Naslov je obavezan.");
      return;
    }

    const urls = mediaUrls.split(",").map((u) => u.trim()).filter(Boolean);
    const tagIdsClean = selectedTagIds.map(String).filter(Boolean);

    setPosting(true);
    try {
      const now = new Date();
      const payload = {
        id: existingPost?.id ?? generateObjectId(),
        authorId: existingPost?.authorId ?? String(sub).trim(),
        title: title.trim(),
        body: body?.trim() || "",
        mediaUrls: urls,
        tagsIds: tagIdsClean,
        likedByUserIds: existingPost?.likedByUserIds ?? [],
        createdAt: existingPost?.createdAt ?? now,
        updatedAt: now,
      };

      const savedPost = existingPost
        ? await updatePost(payload)
        : await apiCreatePost(payload);

      onPost?.(savedPost);

      if (!existingPost) {
        setTitle("");
        setBody("");
        setMediaUrls("");
        setSelectedTagIds([]);
        setTagInput("");
      }

      onCancel?.();
    } catch (ex) {
      console.error(ex);
      setErr("Došlo je do greške prilikom čuvanja posta.");
    } finally {
      setPosting(false);
    }
  };

  const tagPill = (active) => ({
    display: "inline-flex",
    alignItems: "center",
    backgroundColor: active ? "#1F2937" : "#374151",
    color: "#F9FAFB",
    padding: "0.25rem 0.75rem",
    borderRadius: "9999px",
    fontSize: "0.875rem",
    border: active ? "1px solid #FBBF24" : "1px solid #4B5563",
    cursor: "pointer",
    transition: "all 0.2s",
  });

  const filteredTags = categories.filter(
    (c) => c.name.toLowerCase().includes(tagInput.toLowerCase()) && !selectedTagIds.includes(c.id)
  );

  const isModified = existingPost
    ? title !== existingPost.title ||
      body !== existingPost.body ||
      mediaUrls !== (existingPost.mediaUrls?.join(", ") ?? "") ||
      JSON.stringify(selectedTagIds) !== JSON.stringify(existingPost.tagsIds ?? [])
    : true;

  const canSave = title.trim() !== "" && isModified && !posting;

  return (
    <form
      onSubmit={handleSubmit}
      className="w-full max-w-3xl bg-gray-900 rounded-2xl p-6 flex flex-col gap-4 shadow-lg"
    >
      <h2 className="text-2xl font-bold text-yellow-400 text-center mb-4">
        {existingPost ? "Izmeni Post" : "Kreiraj Post"}
      </h2>

      <input
        type="text"
        placeholder="Naslov"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        className="border border-gray-700 rounded-xl px-4 py-2 bg-gray-800 text-white focus:outline-none focus:ring-2 focus:ring-yellow-400 placeholder-gray-400"
      />

      <textarea
        placeholder="Tekst (opciono)"
        value={body}
        onChange={(e) => setBody(e.target.value)}
        className="border border-gray-700 rounded-xl px-4 py-2 bg-gray-800 text-white focus:outline-none focus:ring-2 focus:ring-yellow-400 min-h-[8rem] resize-y placeholder-gray-400"
      />

      <input
        type="text"
        placeholder="Image/Video URLs (comma separated)"
        value={mediaUrls}
        onChange={(e) => setMediaUrls(e.target.value)}
        className="border border-gray-700 rounded-xl px-4 py-2 bg-gray-800 text-white focus:outline-none focus:ring-2 focus:ring-yellow-400 placeholder-gray-400"
      />

      {/* Tagovi sa autocomplete */}
      <div className="grid gap-2 relative">
        <div className="font-semibold text-gray-200">Tagovi</div>
        <input
          type="text"
          placeholder="Dodaj novi tag i pritisni Enter"
          value={tagInput}
          onChange={(e) => setTagInput(e.target.value)}
          onKeyDown={handleAddTag}
          className="border border-gray-700 rounded-xl px-4 py-2 bg-gray-800 text-white focus:outline-none focus:ring-2 focus:ring-yellow-400 placeholder-gray-400 mt-1 w-full"
          autoComplete="off"
        />
        {tagInput.trim() && (
          <div className="absolute z-10 bg-gray-800 border border-gray-700 rounded-xl mt-1 w-full max-h-40 overflow-y-auto shadow-lg">
            {filteredTags.map((c) => (
              <div
                key={c.id}
                onClick={async () => {
                  await ensureTagCreated(c.name);
                  setTagInput("");
                }}
                className="px-3 py-2 cursor-pointer hover:bg-yellow-500 hover:text-gray-900 transition rounded-tl-xl rounded-tr-xl"
              >
                {c.name}
              </div>
            ))}
            {!categories.some(
              (c) => c.name.toLowerCase() === tagInput.toLowerCase()
            ) && (
              <div
                onClick={async () => {
                  await ensureTagCreated(tagInput);
                  setTagInput("");
                }}
                className="px-3 py-2 cursor-pointer hover:bg-yellow-500 hover:text-gray-900 transition rounded-b-xl"
              >
                Dodaj "{tagInput}"
              </div>
            )}
          </div>
        )}

        {/* Selektovani tagovi */}
        <div className="flex flex-wrap gap-2 mt-2">
          {selectedTagIds
            .map((id) => categories.find((c) => c.id === id))
            .filter(Boolean)
            .map((c) => (
              <span
                key={c.id}
                onClick={() => toggleTag(c.id)}
                style={tagPill(true)}
                title="Klik za uklanjanje"
              >
                {c.name} ✓
              </span>
            ))}
        </div>
      </div>

      {err && <div className="text-red-500 text-sm">{err}</div>}

      <div className="flex justify-end gap-2 pt-2">
        <button
          type="button"
          onClick={onCancel}
          className="px-4 py-2 bg-gray-700 text-gray-200 rounded-xl hover:bg-gray-600 transition"
        >
          Odustani
        </button>
        <button
          type="submit"
          disabled={!canSave}
          className={`px-4 py-2 rounded-xl transition ${
            canSave
              ? "bg-yellow-500 text-gray-900 hover:bg-yellow-400"
              : "bg-gray-600 text-gray-400 cursor-not-allowed"
          }`}
        >
          {posting
            ? existingPost
              ? "Ažuriram..."
              : "Objavljujem..."
            : existingPost
            ? "Sačuvaj"
            : "Objavi"}
        </button>
      </div>
    </form>
  );
}