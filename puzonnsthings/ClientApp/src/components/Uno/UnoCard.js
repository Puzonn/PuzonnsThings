import "./UnoGame.css";

export const Card = ({source,clicked}) => 
{
    if(source == null)
    {
        return;
    }
    
    return (
        <div>
            <img onClick={clicked} src={require("./UnoResource/"+source+".png")} alt="card" draggable="false" />
        </div>
    );
}
