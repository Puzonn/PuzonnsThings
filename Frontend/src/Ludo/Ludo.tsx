import { createRef, useEffect, useRef, useState } from "react"
import { Plane } from "../Types/LudoTypes"

export const Ludo = () => 
{
    const canvas = useRef(null)
    const [defaultPlane, setDefaultPlane] = useState<Plane>() 

    useEffect(() => {
        const plane = new Image();
        plane.src = require("./plane.png")
        plane.onload = () => 
        {
            setDefaultPlane({image: plane})

            const context = (canvas.current as any).getContext('2d');
            context.drawImage(defaultPlane?.image, 0, 0)
        }
    },[])

    useEffect(() => 
    {
    }, [defaultPlane])

    return (
        <div>
            <canvas ref={canvas}></canvas>
        </div>
    )
}