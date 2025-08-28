let connection = null;

export function startFriendsHub(onFriendListChanged) {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/friendshub")
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    connection.on("FriendListChanged", () => {
        onFriendListChanged();
    });
    connection.start();
}
