import { useEffect, useState } from "react";
import { getAllCategories, deleteCategory } from "../../api/categoryApi";

export default function CategoryList() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState(null);

  const refresh = async () => {
    setLoading(true);
    setErr(null);
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

  const remove = async (id) => {
    await deleteCategory(id);
    await refresh();
  };

  if (loading) return <div>Učitavanje tagova…</div>;
  if (err) return <div style={{ color: "#e11d48" }}>{err}</div>;

  return (
    <div style={{ display: "grid", gap: 8 }}>
      {items.map((c) => (
        <div key={c.id} style={{ display: "flex", justifyContent: "space-between", border: "1px solid #ddd", padding: "8px 12px", borderRadius: 8 }}>
          <span>{c.name}</span>
          <button onClick={() => remove(c.id)} style={{ color: "#e11d48" }}>Obriši</button>
        </div>
      ))}
      {items.length === 0 && <div style={{ fontSize: 14, color: "#666" }}>Nema tagova.</div>}
    </div>
  );
}
