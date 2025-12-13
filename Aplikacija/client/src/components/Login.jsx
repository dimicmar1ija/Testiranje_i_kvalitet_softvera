import React, { useState, useContext } from "react";
import { useMutation } from "@tanstack/react-query";
import { AuthContext } from "../context/AuthContext";
import { useNavigate } from "react-router-dom";
import axios from "../api/axiosInstance";
import "../app.css"; // use global app.css

const Login = () => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const { login } = useContext(AuthContext);
  const navigate = useNavigate();

  const mutation = useMutation({
    mutationFn: () => axios.post("/Auth/login", { username, password }),
    onSuccess: (res) => {
      login(res.data.token);
      navigate("/home");
    },
    onError: () => alert("Login failed"),
  });

  const handleSubmit = (e) => {
    e.preventDefault();
    mutation.mutate();
  };

  return (
    <div className="form-container">
      <form className="form-box" onSubmit={handleSubmit}>
        <h2>Login</h2>
        <input
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          placeholder="Username"
          required
        />
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder="Password"
          required
        />
        <button type="submit" disabled={mutation.isLoading}>
          {mutation.isLoading ? "Logging in..." : "Login"}
        </button>
        <button
          type="button"
          className="secondary-button"
          onClick={() => navigate("/register")}
        >
          Go to Register
        </button>
      </form>
    </div>
  );
};

export default Login;
