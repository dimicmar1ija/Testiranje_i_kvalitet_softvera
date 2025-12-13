import { useState } from "react";

export default function CommentForm({ postId, parentCommentId = null, onSubmit }) {
  const [body, setBody] = useState("");

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!body.trim()) return;
    onSubmit({ postId, body, parentCommentId });
    setBody("");
  };

  return (
    <form onSubmit={handleSubmit} style={{ textAlign: "left", display: "grid", gap: 8 }}>
      <textarea
        rows={3}
        placeholder={parentCommentId ? "Odgovori na komentar…" : "Napiši komentar…"}
        value={body}
        onChange={(e) => setBody(e.target.value)}
        className="w-full p-3 rounded-xl placeholder-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500"
        style={{
          backgroundColor: "#111827", 
          color: "#facc15",           
          caretColor: "#fbbf24",      
          border: "1px solid #374151", 
        }}
      />
      <button
        type="submit"
        className="px-3 py-1 bg-yellow-500 text-black rounded-full hover:bg-yellow-400 transition-colors"
      >
        {parentCommentId ? "Odgovori" : "Objavi komentar"}
      </button>
    </form>
  );
}
