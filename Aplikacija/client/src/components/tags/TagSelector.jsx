import { useEffect, useState } from "react";
import { getAllCategories } from "../../api/categoryApi";

export default function TagSelector({ value = [], onChange }) {
  const [all, setAll] = useState([]);

  useEffect(() => {
    getAllCategories().then(setAll);
  }, []);

  const toggle = (id) => {
    const set = new Set(value);
    set.has(id) ? set.delete(id) : set.add(id);
    onChange(Array.from(set));
  };

  return (
    <div style={{ display: "grid", gap: 6 }}>
      {all.map((c) => (
        <label key={c.id} style={{ display: "flex", gap: 6, alignItems: "center" }}>
          <input
            type="checkbox"
            checked={value.includes(c.id)}
            onChange={() => toggle(c.id)}
          />
          <span>{c.name}</span>
        </label>
      ))}
    </div>
  );
}
