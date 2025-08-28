let onlineUsers = new Set();
let myUserId = null;
let connection = null;

export function startPresenceHub(currentUserId, onOnlineUsersChanged) {
    myUserId = currentUserId;
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/presencehub")
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    connection.on("UserOnline", userId => {
        onlineUsers.add(userId);
        onOnlineUsersChanged(Array.from(onlineUsers));
    });
    connection.on("UserOffline", userId => {
        onlineUsers.delete(userId);
        onOnlineUsersChanged(Array.from(onlineUsers));
    });
    connection.start().then(() => {
        connection.invoke("GetOnlineUsers").then(users => {
            onlineUsers = new Set(users);
            onOnlineUsersChanged(Array.from(onlineUsers));
        });
    });
}

export function isUserOnline(userId) {
    return onlineUsers.has(userId);
}
