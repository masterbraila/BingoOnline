let connection = null;
let myUserId = null;
let openChats = {};

export function startUserChatHub(currentUserId, onMessageReceived) {
    myUserId = currentUserId;
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/userchathub")
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    connection.on("ReceivePrivateMessage", (fromUserId, message) => {
        onMessageReceived(fromUserId, message);
    });
    connection.start();
}

export function sendPrivateMessage(toUserId, message) {
    if (connection) {
        connection.invoke("SendPrivateMessage", toUserId, message);
    }
}
