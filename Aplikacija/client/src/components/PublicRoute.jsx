// src/components/PublicRoute.jsx
import { Navigate, Outlet } from 'react-router-dom';

//Korisnici koji su logovani ne trebaju da imaju pristup login i register stranicama
const PublicRoute = ({ isAuthenticated }) => {
  if (isAuthenticated) {
    return <Navigate to="/home" replace />;
  }
  return <Outlet />;
};

export default PublicRoute;
