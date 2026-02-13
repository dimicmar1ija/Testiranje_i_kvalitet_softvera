import { useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "../api/axiosInstance";
import "../app.css"; // use global app.css

export const Register = () => {
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const navigate = useNavigate();

  const handleRegister = async (e) => {
    e.preventDefault();
    try {
      await axios.post("Auth/register", { username, email, password });
      navigate("/login");
    } catch (err) {
      alert("Registration failed: " + err);
    }
  };

  return (
    <div className="form-container">
      <form className="form-box" onSubmit={handleRegister}>
        <h2>Register</h2>
        <input
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          placeholder="Username"
          required
        />
        <input
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          placeholder="Email"
          required
        />
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder="Password"
          required
        />
        <button type="submit">Register</button>
        <button
          type="button"
          className="secondary-button"
          onClick={() => navigate("/login")}
        >
          Go to Login
        </button>
      </form>
    </div>
  );
};
