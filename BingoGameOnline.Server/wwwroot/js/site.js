// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function() {
    if (window.isAuthenticated) {
        $.get('/api/account/me', function(me) {
            var myUserId = me.userId;
            import('/js/presence.js').then(mod => {
                mod.startPresenceHub(myUserId, function(onlineIds) {
                    window._onlineUserIds = onlineIds;
                    $(document).trigger('presence:onlineUsersChanged', [onlineIds]);
                });
            });
        });
    }
});
