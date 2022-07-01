import React, { Component } from 'react';
import "./UnoGame.css";
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import {Card} from "./UnoCard";
import $ from 'jquery';
import { ConsoleLogger } from '@microsoft/signalr/dist/esm/Utils';

export class Uno extends Component
{
    constructor(props)
    {
        super(props);
        
        this.state = { PlayerTurn: '', Players: [], Connection: null, joinned: false, gameStarted: false, players: [],cards: [], username: "",CardOnTop: null };
        this.JoinGame = this.JoinGame.bind(this);
        this.GameStarted = this.GameStarted.bind(this);
        this.StartGame = this.StartGame.bind(this); 
        this.SendCard = this.SendCard.bind(this);
        this.UpdateRound = this.UpdateRound.bind(this);
        this.GetNewCard = this.GetNewCard.bind(this);
        this.SetCards = this.SetCards.bind(this);
    }

    GameStarted(StartInfo)
    {
        let _ = JSON.parse(StartInfo);
        console.log(_);
        let Players = [];

        for(let i =0;i<_.Players.length;i++)
        {
            if(_.Players[i]  == _.PlayerTurn)
            {
                Players.push(<p id={"pl"+_.Players[i]} style={{color: 'red'}}>{_.Players[i]}</p>); 
            }
            else{
                Players.push(<p id={"pl"+_.Players[i]} >{_.Players[i]}</p>); 
            } 
           
        }
        this.setState({ Players: Players});
        this.setState(
            {
                cards: _.Cards,
                gameStarted: true,
                CardOnTop: _.CardOnTop.Color+"-"+_.CardOnTop.Power,
            });
        if(_.PlayerTurn == this.state.username)
        {
            this.setState({ PlayerTurn: "Your Turn" });
        }else {this.setState({PlayerTurn: _.PlayerTurn})}
            
    }

    UpdateRound(info)
    {
        let _ = JSON.parse(info);
        console.log(_);

        if(_.PlayerTurn == this.state.username){
            this.setState({PlayerTurn: 'Your turn'});
        }
        else{
            this.setState({PlayerTurn: _.PlayerTurn});
        }

        this.setState({ CardOnTop: _.CardOnTop.Color+"-"+_.CardOnTop.Power });
        
        for(let i =0;i<this.state.Players.length;i++)
        {
            if(this.state.Players[i].props.id == "pl"+_.PlayerTurn)
            {
                this.state.Players[i].style.color = 'red'
            }
            else{
                this.state.Players[i].style.color = 'white'
            }
        }
    }

    SetPlayerTurn(PlayerInfo)
    {
        let info = JSON.parse(PlayerInfo);
        this.setState({cards: info.Cards});
    }

    SetSpecTurn(SpecInfo)
    {
        let info = JSON.parse(SpecInfo);
        console.log(info);
    }

    StartGame()
    { 
        this.state.Connection.invoke("StartGame");
    }

    SetCards(info)
    {
        console.log(info);
        let _ = JSON.parse(info);
        this.setState({cards: _});//_ == cards itself TODO: add only one card     
    }

    SendCard(card)
    {
        console.log(card);
        this.state.Connection.invoke("RegisterCard",this.state.Connection.connection.connectionId,card);
    }
    
    GetNewCard()
    {
        this.state.Connection.invoke("GetNewCard",this.state.Connection.connection.connectionId);
    }

    componentWillUnmount()
    {
        if(this.state.Connection != null){
            this.state.Connection.stop()
        }
    }

    JoinGame()
    {
        this.state.Connection = new HubConnectionBuilder()
            .withUrl('http://localhost:5158/unoHub', {
            })
            .configureLogging(LogLevel.Critical)
            .build();

        this.setState({ joinned: true });

        let LobbyInfo;
        let Players;

        this.state.Connection.start().then(() =>
        {
            this.state.Connection.invoke("PlayerJoinned", this.state.username,this.state.Connection.connection.connectionId);
            this.state.Connection.on("GameInfoCallback", (info) =>
            {
                LobbyInfo = JSON.parse(info);
                Players = LobbyInfo.Players;
           
                for (let i = 0; i < 4; i++) 
                {
                    if (Players[i] === undefined)
                    {
                        document.getElementById("lbpl" + i).textContent = " STATUS: NOT JOINNED";
                        document.getElementById("lbp" + i).style['color'] = 'red';
                    }
                    else 
                    {
                        document.getElementById("lbpl" + i).textContent = "Player connected: "+Players[i].PlayerName;
                        document.getElementById("lbp" + i).style['color'] = 'green';
                    }
                }
            })
            this.state.Connection.on("UpdateRound",(info) => this.UpdateRound(info));
            this.state.Connection.on("GameStarted", (PlayerInfo) => this.GameStarted(PlayerInfo));

            this.state.Connection.on("StartGame", (info) => this.GameStarted(info));

            this.state.Connection.on("SetPlayerTurn", (PlayerInfo) => this.SetPlayerTurn(PlayerInfo));
            this.state.Connection.on("debug", (info) => console.log(info));
            this.state.Connection.on("SetSpecTurn", (info) => this.SetSpecTurn(info));
            this.state.Connection.on("SetCards", (info) => this.SetCards(info));
        })
    }

    render() {
        if (!this.state.joinned)
        {
            return (
                <div>
                    <input onChange={(e) => this.setState({ username: e.target.value })} />
                    <button onClick={this.JoinGame} style={{ width: 50, height: 20 }} />
                </div>
            );  
        }
        else if (!this.state.gameStarted && this.state.joinned) {
            return (
                <div className="container">
                    <div className="LobbyContainer">
                        <h1 className="LobbyText">Uno Lobby </h1>
                    </div>

                    <div className="LobbyTextContainer">

                        <div className='LobbyDisplay'>
                            <h2> Player ONE</h2>
                            <p id="lbpl0"> STATUS: NOT JOINNED </p>
                            <hr id="lbp0" style={{height:2, width: 200,color: 'red'}}/>
                        </div>

                        <div  className='LobbyDisplay'>
                            <h2> Player TWO</h2>
                            <p id="lbpl1"> STATUS: NOT JOINNED </p>
                            <hr id="lbp1" style={{height:2, width: 200,color: 'red'}}/>
                        </div>
                        <div  className='LobbyDisplay'>
                            <h2> Player THREE</h2>
                            <p id="lbpl2"> STATUS: NOT JOINNED </p>
                            <hr id="lbp2" style={{height:2, width: 200,color: 'red'}}/>
                        </div>
                        <div  className='LobbyDisplay'>
                            <h2> Player FOUR</h2>
                            <p id="lbpl3"> STATUS: NOT JOINNED </p>
                            <hr id="lbp3" style={{height:2, width: 200,color: 'red'}}/>
                            <button onClick={this.StartGame} title="uwu" placeholder="Start game" className="btnStartGame" />   
                        </div>
                    </div>
                </div>
            );
        }
        else
        {
            return (
                
            <div>
                <div className='PlayerTurnAndNext'>
                 <p> Player Turn: {this.state.PlayerTurn} </p>
                </div>
                <div> 
                    <img onClick={() => this.GetNewCard()} src={require("./UnoResource/uno.png")}/>
                </div>
                <div className="container">
                </div>
            <div className="CardOnTop">
                <Card id="cardontop" source={this.state.CardOnTop}/>
                <div className="CardContainer">
                    {this.state.cards.map((card,index) =>
                    (
                        <Card key={index} source={card.Color+"-"+card.Power} clicked={() => this.SendCard(card)}/>
                    ))}
                </div>
            </div>
            </div>
            );
        }
    }
}
