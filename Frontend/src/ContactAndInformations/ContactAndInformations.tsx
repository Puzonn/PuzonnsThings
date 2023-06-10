import "./ContactAndInformations.css";
import { useState } from "react";

export const ContactAndInformations = () => {
  return (
    <div id="cai">
      <div id="cai-main">
        <h4>About and informations</h4>
        <p>
          Email:{" "}
          <a
            href="mailto:przemekpyszkiewicz@gmail.com"
            style={{ color: "var(--color-blue)" }}
          >
            przemekpyszkiewicz@gmail.com
          </a>
        </p>
        <p>
          Site's Github:{" "}
          <a
            style={{ color: "var(--color-blue)" }}
            href="https://github.com/Puzonn/PuzonnsThings"
          >
            Github
          </a>
        </p>
      </div>

      <div id="cai-about">
        <h4>About site</h4>
        <div style={{ paddingLeft: "25px" }}>
          <span>
            The website is built in ReactJS with TypeScript. It features various
            mini games and many other attractions. The backend is developed in
            C#, and SignalR was used for websockets. Bearer authentication is
            employed for user authentication.
          </span>
        </div>
      </div>

      <div id="cai-content">
        <div>
          <h3>Todo: </h3>
          <ul>
            <li>Creating.</li>
            <li>Deleting.</li>
            <li>Updating.</li>
            <li>Sorting.</li>
            <li>Searching.</li>
          </ul>
        </div>
        <div>
          <h3>Lobby System: </h3>
          <ul>
            <li>Creating a lobby with any content.</li>
            <li>Automatically deleting lobby after period time.</li>
          </ul>
        </div>
        <div>
          <h3>Fully Implemented Contents: </h3>
          <ul>
            <li>Yahtzee.</li>
            <li>WatchTogether.</li>
            <li>Todo WebApp.</li>
            <li>Wheel Of Fortune.</li>
          </ul>
        </div>
      </div>
    </div>
  );
};
