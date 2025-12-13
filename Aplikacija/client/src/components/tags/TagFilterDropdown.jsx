import { useEffect, useState } from "react";
import { getAllCategories } from "../../api/categoryApi";

export default function TagFilterSidebar({
  appliedSelected = [],
  onApply,
  onReset,
}) {
  const [cats, setCats] = useState([]);
  const [open, setOpen] = useState(true);
  const [tempSelected, setTempSelected] = useState(appliedSelected);

  useEffect(() => { getAllCategories().then(setCats); }, []);
  useEffect(() => { setTempSelected(appliedSelected); }, [appliedSelected]);

  const toggle = (id) => {
    const s = new Set(tempSelected);
    s.has(id) ? s.delete(id) : s.add(id);
    setTempSelected(Array.from(s));
  };

  return (
    <div className="sticky top-4 grid gap-2">
      <button
        onClick={() => setOpen((o) => !o)}
        className="w-full text-left px-4 py-2 rounded-full bg-yellow-500 text-white font-semibold "
      >
        Tagovi {open ? "▾" : "▸"}
      </button>

      {open && (
        <div className="border border-gray-200 rounded-lg p-4 bg-white grid gap-3">
          <div className="grid gap-2 max-h-72 overflow-auto pr-1">
            {cats.map((c) => (
              <label key={c.id} className="flex items-center gap-2 text-sm text-gray-800">
                <input
                  type="checkbox"
                  checked={tempSelected.includes(c.id)}
                  onChange={() => toggle(c.id)}
                  className="accent-yellow-600 w-4 h-4 focus:ring-yellow-500"
                />
                <span>{c.name}</span>
              </label>
            ))}
            {!cats.length && <div className="text-gray-500 text-sm">Nema tagova.</div>}
          </div>

          <div className="flex gap-2">
            <button
              onClick={() => onApply?.(tempSelected)}
              className="px-4 py-2 rounded-full bg-yellow-600 text-white font-semibold hover:bg-yellow-500 transition-colors"
            >
              Filtriraj
            </button>
            <button
              onClick={() => {
                setTempSelected([]);
                onReset?.();
              }}
              className="px-4 py-2 rounded-full bg-red-600 text-white font-semibold hover:bg-red-500 transition-colors"
            >
              Ukloni filtere
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
