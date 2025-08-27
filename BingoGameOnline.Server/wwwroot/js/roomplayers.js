// This script connects to the SignalR RoomHub and updates the player list in the room in real time
const roomId = parseInt(window.location.pathname.split("/").pop());
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/roomhub")
    .build();

connection.on("PlayerListChanged", function () {
    // Reload only the player list section
    fetch(window.location.pathname + "?handler=Players")
        .then(response => response.text())
        .then(html => {
            document.getElementById("player-list-section").innerHTML = html;
        });
});

connection.start().then(function () {
    connection.invoke("JoinRoomGroup", roomId);
}).catch(function (err) {
    return console.error(err.toString());
});

window.addEventListener("beforeunload", function () {
    connection.invoke("LeaveRoomGroup", roomId);
});
