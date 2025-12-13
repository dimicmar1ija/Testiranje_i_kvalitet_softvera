import { NavLink } from "react-router";

export function Login() {
    return (
        <div>
            <h1>Login Page</h1>
            <p>Please enter your credentials to log in.</p>

            <input type="text" placeholder="Username" />
            <input type="password" placeholder="Password" />

            <NavLink to="/"> Link </NavLink>

        </div>


    );
}