import { useEffect, useState } from "react";
import { getAllCategories, createCategory, deleteCategory } from "../api/categoryApi";

export default function AdminCategories() {
  const [items, setItems] = useState([]);
  const [name, setName] = useState("");
  const [err, setErr] = useState(null);
  const [loading, setLoading] = useState(false);

  const refresh = async () => {
    setErr(null);
    setLoading(true);
    try {
      const data = await getAllCategories();
      setItems(data);
    } catch {
      setErr("Ne mogu da učitam tagove.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { refresh(); }, []);

  const onCreate = async (e) => {
    e.preventDefault();
    if (!name.trim()) return;
    setErr(null);
    try {
      await createCategory({ name });
      setName("");
      refresh();
    } catch (ex) {
      if (ex?.response?.status === 409) setErr("Tag sa tim imenom već postoji.");
      else setErr("Greška pri kreiranju taga.");
    }
  };

  const onDelete = async (id) => {
    await deleteCategory(id);
    refresh();
  };

  return (
    <div style={{ textAlign: "left", display: "grid", gap: 12 }}>
      <h2>Tagovi</h2>

      <form onSubmit={onCreate} style={{ display: "flex", gap: 8 }}>
        <input
          placeholder="Novi tag"
          value={name}
          onChange={(e) => setName(e.target.value)}
        />
        <button>Dodaj</button>
      </form>
      {err && <div style={{ color: "#e11d48" }}>{err}</div>}

      {loading ? (
        <div>Učitavanje…</div>
      ) : (
        <div style={{ display: "grid", gap: 8 }}>
          {items.map((c) => (
            <div key={c.id} style={{ display: "flex", justifyContent: "space-between", border: "1px solid #ddd", padding: 8, borderRadius: 6 }}>
              <span>{c.name}</span>
              <button onClick={() => onDelete(c.id)} style={{ color: "#e11d48" }}>Obriši</button>
            </div>
          ))}
          {items.length === 0 && <div style={{ color: "#666" }}>Nema tagova.</div>}
        </div>
      )}
    </div>
  );
}
