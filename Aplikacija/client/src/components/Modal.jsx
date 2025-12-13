import React from "react";

export default function Modal({ children, onClose, modalWidth = "max-w-3xl" }) {
  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center"
      style={{ backdropFilter: "blur(6px)", backgroundColor: "rgba(0,0,0,0.4)" }}
      onClick={onClose} // klik na pozadinu zatvara modal
    >
      <div
        className={`bg-white rounded-2xl shadow-xl p-6 w-full ${modalWidth}`}
        onClick={(e) => e.stopPropagation()} // sprečava zatvaranje kad se klikne unutar forme
      >
        {children}
      </div>
    </div>
  );
}
