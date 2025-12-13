export default function TagChips({ tags = [] }) {
  if (!tags?.length) return null;

  const renderName = (t) => (typeof t === "string" ? t : (t.name || t.id));

  return (
    <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
      {tags.map((t) => (
        <span
          key={typeof t === "string" ? t : (t.id || t.name)}
          style={{
            padding: "2px 8px",
            borderRadius: 999,
            background: "#eee",
            fontSize: 12
          }}
        >
          {renderName(t)}
        </span>
      ))}
    </div>
  );
}
