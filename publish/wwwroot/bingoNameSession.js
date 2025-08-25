window.bingoNameSession = {
    setName: function(name) {
        sessionStorage.setItem('bingoPlayerName', name);
    },
    getName: function() {
        return sessionStorage.getItem('bingoPlayerName');
    },
    clearName: function() {
        sessionStorage.removeItem('bingoPlayerName');
    },
    setTicket: function(ticketJson) {
        sessionStorage.setItem('bingoTicket', ticketJson);
    },
    getTicket: function() {
        return sessionStorage.getItem('bingoTicket');
    },
    clearTicket: function() {
        sessionStorage.removeItem('bingoTicket');
    },
    setAdmin: function() {
        sessionStorage.setItem('bingoAdmin', '1');
    },
    isAdmin: function() {
        return sessionStorage.getItem('bingoAdmin') === '1';
    },
    clearAdmin: function() {
        sessionStorage.removeItem('bingoAdmin');
    }
};
