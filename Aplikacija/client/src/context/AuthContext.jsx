import React, { createContext, useContext, useState, useEffect } from "react";
import {jwtDecode} from "jwt-decode";
import { getUserProfile } from "../api/userApi";

// eslint-disable-next-line react-refresh/only-export-components
export const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [token, setToken] = useState(localStorage.getItem("jwt") || null);
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [isAdmin, setIsAdmin] = useState(false);

  const decodeToken = (jwt) => {
    try {
      const decoded = jwtDecode(jwt);
      const roleClaim = decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
      setIsAdmin(roleClaim === "admin");
      return decoded;
    } catch (err) {
      console.error("Failed to decode JWT:", err);
      setIsAdmin(false);
      return null;
    }
  };

  const fetchUser = async (sub) => {
    try {
      const userData = await getUserProfile(sub);
      setUser(userData);
    } catch (err) {
      console.error("Failed to fetch user profile:", err);
      setUser(null);
    }
  };

  const login = async (jwt) => {
    setToken(jwt);
    localStorage.setItem("jwt", jwt);

    const decoded = decodeToken(jwt);
    if (decoded) {
      await fetchUser(decoded.sub);
    }
  };

  const logout = () => {
    setToken(null);
    setUser(null);
    setIsAdmin(false);
    localStorage.removeItem("jwt");
  };

  useEffect(() => {
    const loadUser = async () => {
      if (token) {
        const decoded = decodeToken(token);
        if (decoded) {
          await fetchUser(decoded.sub);
        }
      }
      setLoading(false);
    };
    loadUser();
  }, [token]);

  return (
    <AuthContext.Provider
      value={{
        token,
        user,
        login,
        logout,
        isAdmin,
        isAuthenticated: !!token,
        loading,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = () => useContext(AuthContext);
