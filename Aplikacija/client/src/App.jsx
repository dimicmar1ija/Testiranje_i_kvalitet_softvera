import './App.css';
import Login from './components/Login';
import { Register } from './components/Register';
import { Home } from './pages/home';
import AdminCategories from './pages/AdminCategories';
import TestComments from './pages/TestComments';
import { Routes, Route } from 'react-router-dom';
import ProtectedRoute from './components/ProtectedRoute';
import PublicRoute from './components/PublicRoute';
import { useAuth } from "./context/AuthContext";
import Layout from './components/layout';
import { CreatePost } from "./pages/CreatePost";  
import { MyProfile } from './components/MyProfile';
import { UserProfile } from './components/UserProfile';

function App() {
  const { isAuthenticated } = useAuth();

  return (
    <Routes>
      {/* Javne rute */}
      <Route element={<PublicRoute isAuthenticated={isAuthenticated} />}>
        <Route path="/" element={<Login />} />
        <Route path="login" element={<Login />} />
        <Route path="register" element={<Register />} />
      </Route>

      {/* Zaštićene rute */}
      <Route element={<ProtectedRoute isAuthenticated={isAuthenticated} />}>
        <Route element={<Layout />}>
          <Route path="home" element={<Home />} />
          <Route path="test/comments" element={<TestComments />} />
          <Route path="admin/categories" element={<AdminCategories />} />
          <Route path="create-post" element={<CreatePost />} />
          <Route path="profile" element={<MyProfile />} />
          <Route path="/users/:id" element={<UserProfile />} />
        </Route>
      </Route>
    </Routes>
  );
}

export default App;
