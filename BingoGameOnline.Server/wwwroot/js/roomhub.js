// This script connects to the SignalR RoomHub and reloads the page when the room list changes
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/roomhub")
    .build();

connection.on("RoomListChanged", function () {
    location.reload();
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});
