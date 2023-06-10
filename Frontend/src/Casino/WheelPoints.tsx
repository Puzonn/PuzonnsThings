import { useEffect, useState } from "react";
import { WheelPoint } from "../Types/CasinoTypes";

export const WheelPoints = () => {
  const [points, setPoints] = useState<WheelPoint[]>([]);

  useEffect(() => {
    const Points: WheelPoint[] = [];
    const colors: string[] = [
      "grey",
      "red",

      "grey",
      "blue",

      "grey",
      "red",

      "grey",
      "red",

      "grey",
      "red",

      "grey",
      "red",

      "grey",
      "red",

      "blue",
      "red",

      "grey",
      "red",

      "blue",
      "yellow",
      "blue",
    ];
    
    const rotateDeg = 360 / colors.length;

    for (let i = 0; i < colors.length; i++) {
      const color = colors[i];

      if (color == "red") {
        Points.push({
          color: color,
          trueColor: "#D22B2B",
          rotateDeg: rotateDeg * i,
        });
      } else if (color == "blue") {
        Points.push({
          color: color,
          trueColor: "#4FA1CA",
          rotateDeg: rotateDeg * i,
        });
      } else if (color == "yellow") {
        Points.push({
          color: color,
          trueColor: "#E4D00A",
          rotateDeg: rotateDeg * i,
        });
      } else if (color == "grey") {
        Points.push({
          color: color,
          trueColor: "#808080",
          rotateDeg: rotateDeg * i,
        });
      }
    }

    setPoints(Points);
  }, []);
  return (
    <div id="ss-container">
      {points.map((point, index) => {
        return (
          <div
            key={`point_${index}`}
            style={{
              backgroundColor: `${point.trueColor}`,
              rotate: `${point.rotateDeg}deg`,
            }}
            className={`ss-sector wheel_tag_${point.color}`}
          ></div>
        );
      })}
    </div>
  );
};
