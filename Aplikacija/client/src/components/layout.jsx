import { Outlet } from "react-router-dom";
import Header from "./Header";

// Layout prikazuje Header i sadržaj samo za prijavljene korisnike
function Layout() {
  return (
    <div className="flex flex-col min-h-screen">
      <Header />
      <main className="flex-1 p-4 pt-20">{/* pt-20 zbog fiksnog headera */}
        <Outlet />
      </main>
    </div>
  );
}

export default Layout;
