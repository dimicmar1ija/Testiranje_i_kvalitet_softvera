import { useState } from "react";
import { createCategory } from "../../api/categoryApi";

export default function CategoryCreateForm({ onCreated }) {
  const [name, setName] = useState("");
  const [err, setErr] = useState(null);

  const submit = async (e) => {
    e.preventDefault();
    setErr(null);
    try {
      await createCategory({ name });
      setName("");
      onCreated?.();
    } catch (error) {
      if (error?.response?.status === 409) setErr("Tag sa tim imenom već postoji.");
      else setErr("Greška pri kreiranju taga.");
    }
  };

  return (
    <form onSubmit={submit} style={{ display: "flex", gap: 8, alignItems: "center", justifyContent: "center" }}>
      <input
        placeholder="Novi tag"
        value={name}
        onChange={(e) => setName(e.target.value)}
      />
      <button>Dodaj</button>
      {err && <span style={{ color: "#e11d48", fontSize: 14 }}>{err}</span>}
    </form>
  );
}
