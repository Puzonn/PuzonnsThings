import { useState, useEffect } from "react";
import axios from "axios";
import "./Auth.css";
import { Auth } from "./Auth";
import { Base } from "../Shared/Config";
import { useSearchParams } from "react-router-dom";

export const Login = () => {
  const [error, setError] = useState<string>("");
  const [info, setInfo] = useState<string>("");

  const [searchParams] = useSearchParams({});

  useEffect(() => {
    Auth.IsLoggedInWithRedirect("/todo");
    if (searchParams.get("redirectFrom") != null) {
      setInfo("Your account has been created. \n You can now Log in");
    }
  }, []);

  const HandleLoginFormSubmit = (event: any) => {
    event.preventDefault();

    setError("");

    const username = event.target[0].value;
    const password = event.target[1].value;

    const loginModel = {
      username: username,
      password: password,
    };

    axios
      .post(Base.BASE_URL + "/api/auth/login", JSON.stringify(loginModel), {
        headers: {
          "Content-Type": "application/json",
        },
        withCredentials: true,
      })
      .then((response) => {
        if (response.status === 200) {
          window.location.href = "/todo";
        }
      })
      .catch((x) => 
      {
        if(x.code === 'ERR_NETWORK'){
          setError("No connection to server");
        }
        else{
          setError(x.response.data);
        }
      });
  };

  return (
    <form onSubmit={HandleLoginFormSubmit} className="login-form">
      <h2>Welcome to Puzonns Things!</h2>
      <span style={{ color: "#ffffff99" }}>Log In or Register</span>
      <div>
        <input name="username" required placeholder="Username" type="text" />
      </div>
      <div>
        <input
          name="password"
          required
          placeholder="Password"
          type="password"
          autoComplete="false"
        />
      </div>
      <button value="Login" name="submit" type="submit">
        Login
      </button>
      <div>
        <a>{info}</a>
      </div>
      <div>
        <a style={{ color: "red" }}>{error}</a>
      </div>
      <br />
      <a href="/register">
        Already have account? <span>Create account</span>
      </a>
    </form>
  );
};
