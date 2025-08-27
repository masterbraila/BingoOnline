// Room chat SignalR client
const chatRoomId = parseInt(window.location.pathname.split("/").pop());
const chatUser = document.querySelector(".fw-bold.text-primary")?.textContent || "Guest";
const chatConnection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .build();

chatConnection.on("ReceiveMessage", function (user, message) {
    const msgDiv = document.getElementById("chat-messages");
    const msg = document.createElement("div");
    msg.innerHTML = `<strong>${user}:</strong> ${message}`;
    msgDiv.appendChild(msg);
    msgDiv.scrollTop = msgDiv.scrollHeight;
});

chatConnection.start().then(function () {
    chatConnection.invoke("JoinRoom", chatRoomId);
});

document.getElementById("chat-form").addEventListener("submit", function () {
    const input = document.getElementById("chat-input");
    const message = input.value.trim();
    if (message.length > 0) {
        chatConnection.invoke("SendMessage", chatRoomId, chatUser, message);
        input.value = "";
    }
});

window.addEventListener("beforeunload", function () {
    chatConnection.invoke("LeaveRoom", chatRoomId);
});
