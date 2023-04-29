import axios from "axios";
import { useState, useEffect } from "react";
import { Auth } from "./Auth";
import { Base } from "../Shared/Config";

export const Register = () => {
  const [error, setError] = useState<string>("");
  useEffect(() => {
    axios.defaults.withCredentials = true;
    Auth.IsLoggedInWithRedirect("/todo");
  });

  const HandleRegisterFormSubmit = (event: any) => {
    event.preventDefault();

    const username = event.target[0].value;
    const email = event.target[1].value;
    const password = event.target[2].value;

    const registerModel = {
      email: email,
      password: password,
      username: username,
    };

    axios
      .post(
        Base.BASE_URL + "/api/auth/register",
        JSON.stringify(registerModel),
        {
          headers: {
            "Content-Type": "application/json",
          },
        }
      )
      .then((x) => {
        if (x.status === 200) {
          window.location.href = `/login?redirectFrom=register`;
        }
      })
      .catch((x) => {
        setError("User with given username already exist");
      });
  };

  return (
    <form onSubmit={HandleRegisterFormSubmit} className="login-form">
      <h2>Welcome to Puzonns Things!</h2>
      <span style={{ color: "#ffffff99" }}>Create your account here</span>
      <div>
        <input name="username" required placeholder="Username" type="text" />
      </div>
      <div>
        <input name="email" placeholder="Optional Email" type="text" />
      </div>
      <div>
        <input
          name="password"
          required
          autoComplete="false"
          placeholder="Password"
          type="password"
        />
      </div>
      <button name="submit" type="submit">
        Create Account
      </button>
      <div>
        <a style={{ color: "red" }}>{error}</a>
      </div>
      <a href="/login">
        {" "}
        Already have account? <span>Click here</span>{" "}
      </a>
    </form>
  );
};
